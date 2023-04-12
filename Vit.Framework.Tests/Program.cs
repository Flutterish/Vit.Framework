using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Vit.Framework.Graphics.Parsing.WaveFront;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Graphics.Vulkan.Buffers;
using Vit.Framework.Graphics.Vulkan.Queues;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vit.Framework.Graphics.Vulkan.Shaders;
using Vit.Framework.Graphics.Vulkan.Synchronisation;
using Vit.Framework.Graphics.Vulkan.Textures;
using Vit.Framework.Graphics.Vulkan.Windowing;
using Vit.Framework.Input;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Platform;
using Vit.Framework.Text.Fonts.OpenType;
using Vit.Framework.Threading;
using Vit.Framework.Windowing;
using Vit.Framework.Windowing.Sdl;
using Vulkan;
using Image = Vit.Framework.Graphics.Vulkan.Textures.Image;
using Semaphore = Vit.Framework.Graphics.Vulkan.Synchronisation.Semaphore;

namespace Vit.Framework.Tests;

public class Program : App {
	public Program () : base( "Test App" ) { }

	public static void Main () {
		var font = OpenTypeFont.FromStream( File.OpenRead( @"D:\Main\Solutions\Git\fontineer\sample-fonts\Maria Aishane Script.otf" ) );
		/*
		var app = new Program();
		app.ThreadRunner.ThreadingMode = ThreadingMode.Multithreaded;
		using var host = new SdlHost( app );
		app.Run( host );
		*/
	}

	protected override void Initialize ( Host host ) {
		var a = host.CreateWindow( RenderingApi.Vulkan, this );
		a.Title = "Window A [Vulkan]";
		a.Initialized += _ => {
			ThreadRunner.RegisterThread( new RenderThread( a, host, a.Title ) );
		};
		//var b = host.CreateWindow( RenderingApi.Vulkan, this );
		//b.Title = "Window B [Vulkan]";
		//b.Initialized += _ => {
		//	ThreadRunner.RegisterThread( new RenderThread( b, host, b.Title ) );
		//};
		//var c = host.CreateWindow( RenderingApi.Vulkan, this );
		//c.Title = "Window C [Vulkan]";
		//c.Initialized += _ => {
		//	ThreadRunner.RegisterThread( new RenderThread( c, host, c.Title ) );
		//};

		Task.Run( async () => {
			while ( !a.IsClosed /*|| !b.IsClosed || !c.IsClosed*/ )
				await Task.Delay( 1 );

			Quit();
		} );
	}

	class RenderThread : AppThread {
		VulkanRenderer renderer;
		VulkanInstance vulkan;
		Host host;
		Window window;
		DateTime startTime;
		DateTime lastFrameTime;
		public RenderThread ( Window window, Host host, string name ) : base( name ) {
			lastFrameTime = startTime = DateTime.Now;
			this.host = host;
			this.window = window;
			renderer = (VulkanRenderer)host.CreateRenderer( RenderingApi.Vulkan, new[] { RenderingCapabilities.DrawToWindow }, new() {
				Window = window
			} )!;
			vulkan = renderer.Instance;

			window.Resized += onWindowResized;

			window.PhysicalKeyDown += Keys.Add;
			window.PhysicalKeyUp += Keys.Remove;
		}

		bool windowResized;
		void onWindowResized ( Window _ ) {
			windowResized = true;
		}

		BinaryStateTracker<Key> Keys = new();

		Device device = null!;
		SwapchainInfo swapchainInfo = null!;
		Swapchain swapchain = null!;
		ShaderModule vertex = null!;
		ShaderModule fragment = null!;
		ShaderModule compute = null!;
		RenderPass renderPass = null!;
		Pipeline pipeline = null!;
		CommandPool commandPool = null!;
		CommandPool copyCommandPool = null!;
		DeviceBuffer<float> vertexBuffer = null!;
		DeviceBuffer<uint> indexBuffer = null!;
		Sampler sampler = null!;
		struct Matrices {
			public Matrix4<float> Model;
			public Matrix4<float> View;
			public Matrix4<float> Projection;
		}
		HostBuffer<Matrices> uniforms = null!;
		Image texture = null!;
		VkClearColorValue bg;

		VkQueue graphicsQueue;
		VkQueue presentQueue;

		SimpleObjModel model = null!;
		protected override void Initialize () {
			var surface = ( (IVulkanWindow)window ).GetSurface( vulkan );
			var (physicalDevice, info) = vulkan.GetBestDeviceInfo( surface );
			swapchainInfo = info;

			device = physicalDevice.CreateDevice( SwapchainInfo.RequiredExtensionsCstr, new CString[] { }, new[] {
				info.GraphicsQueue,
				info.PresentQueue
			} );
			graphicsQueue = device.GetQueue( swapchainInfo.GraphicsQueue );
			presentQueue = device.GetQueue( swapchainInfo.PresentQueue );
			swapchain = device.CreateSwapchain( surface, info.SelectBest(), window.PixelSize );

			vertex = device.CreateShaderModule( new SpirvBytecode( @"#version 450
				layout(location = 0) in vec3 inPosition;
				layout(location = 1) in vec3 inColor;
				layout(location = 2) in vec2 inTexCoord;

				layout(location = 0) out vec3 fragColor;
				layout(location = 1) out vec2 fragTexCoord;

				layout(binding = 0) uniform Matrices {
					mat4 model;
					mat4 view;
					mat4 projection;
				} matrices;

				void main() {
					gl_Position = matrices.projection * matrices.view * matrices.model * vec4(inPosition, 1.0);
					fragColor = inColor;
					fragTexCoord = inTexCoord;
				}
			", ShaderLanguage.GLSL, ShaderPartType.Vertex ) );

			fragment = device.CreateShaderModule( new SpirvBytecode( @"#version 450
				layout(location = 0) in vec3 fragColor;
				layout(location = 1) in vec2 fragTexCoord;

				layout(location = 0) out vec4 outColor;

				layout(binding = 1) uniform sampler2D texSampler;

				void main() {
					outColor = texture(texSampler, fragTexCoord) * vec4(fragColor, 1);
				}
			", ShaderLanguage.GLSL, ShaderPartType.Fragment ) );

			var depthFormat = device.PhysicalDevice.GetBestSupportedFormat(
				new[] { VkFormat.D32Sfloat, VkFormat.D32SfloatS8Uint, VkFormat.D24UnormS8Uint },
				VkFormatFeatureFlags.DepthStencilAttachment
			);

			renderPass = new RenderPass( device, device.PhysicalDevice.GetMaxColorDepthMultisampling(), swapchain.Format.Format.format, depthFormat );
			pipeline = new Pipeline( device, new[] { vertex, fragment }, renderPass );
			swapchain.SetRenderPass( renderPass );

			commandPool = device.CreateCommandPool( info.GraphicsQueue );
			copyCommandPool = device.CreateCommandPool( info.GraphicsQueue, VkCommandPoolCreateFlags.ResetCommandBuffer | VkCommandPoolCreateFlags.Transient );
			frameInfo = new() {
				ImageAvailable = device.CreateSemaphore(),
				RenderFinished = device.CreateSemaphore(),
				InFlight = device.CreateFence( signaled: true ),
				Commands = commandPool.CreateCommandBuffer()
			};

			var rng = new Random();
			bg = new( rng.NextSingle(), rng.NextSingle(), rng.NextSingle() );
			bg = new( 0, 0, 0 );

			model = SimpleObjModel.FromLines( File.ReadLines( "./viking_room.obj" ) );

			vertexBuffer = new( device, VkBufferUsageFlags.VertexBuffer );
			vertexBuffer.AllocateAndTransfer( model.Vertices.SelectMany( x => new[] {
					x.Position.X,
					x.Position.Y,
					x.Position.Z,
					1,
					1,
					1,
					x.TextureCoordinates.X,
					x.TextureCoordinates.Y
				} ).ToArray(), 
				copyCommandPool, graphicsQueue 
			);

			indexBuffer = new( device, VkBufferUsageFlags.IndexBuffer );
			indexBuffer.AllocateAndTransfer( model.Indices.Select( x => (uint)x ).ToArray(), copyCommandPool, graphicsQueue );

			uniforms = new( device, VkBufferUsageFlags.UniformBuffer );
			uniforms.Allocate( 1 );
			pipeline.DescriptorSet.ConfigureUniforms( uniforms, 0 );

			using var image = SixLabors.ImageSharp.Image.Load<Rgba32>( "./viking_room.png" );
			image.Mutate( x => x.Flip( FlipMode.Vertical ) );
			texture = new( device );
			texture.AllocateAndTransfer( image, copyCommandPool, graphicsQueue );

			sampler = new( device, maxLod: texture.MipMapLevels );
			pipeline.DescriptorSet.ConfigureTexture( texture, sampler, 1 );
		}

		FrameInfo frameInfo;
		protected override void Loop () {
			if ( windowResized ) {
				windowResized = false;
				recreateSwapchain();
			}

			record( frameInfo );
		}

		struct FrameInfo : IDisposable {
			public required Semaphore ImageAvailable;
			public required Semaphore RenderFinished;
			public required Fence InFlight;
			public required CommandBuffer Commands;

			public void Dispose () {
				ImageAvailable.Dispose();
				RenderFinished.Dispose();
				InFlight.Dispose();
			}
		};

		Point3<float> position = new( 0, 0, -1 );
		void record ( FrameInfo info ) {
			var commands = info.Commands;
			info.InFlight.Wait();

			if ( !validateSwapchain( swapchain.GetNextFrame( info.ImageAvailable, out var frame, out var index ), recreateSuboptimal: false ) ) {
				return;
			}

			info.InFlight.Reset();

			commands.Reset();
			commands.Begin();
			commands.BeginRenderPass( frame, new VkClearValue { color = bg }, new VkClearValue { depthStencil = { depth = 1 } } );
			commands.BindPipeline( pipeline );
			commands.SetViewPort( new() {
				minDepth = 0,
				maxDepth = 1,
				width = frame.Size.width,
				height = frame.Size.height
			} );
			commands.SetScissor( new() {
				extent = frame.Size
			} );
			commands.BindVertexBuffer( vertexBuffer );
			commands.BindIndexBuffer( indexBuffer );
			var now = DateTime.Now;
			var deltaTime = (float)(now - lastFrameTime).TotalSeconds;

			var cameraRotation = Matrix4<float>.FromAxisAngle( Vector3<float>.UnitX, ( (float)window.CursorPosition.Y / window.Height - 0.5f ).Degrees() * 180 )
				* Matrix4<float>.FromAxisAngle( Vector3<float>.UnitY, ( (float)window.CursorPosition.X / window.Width - 0.5f ).Degrees() * 360 );

			var inverseCameraRotation = cameraRotation.Inverse();

			var deltaPosition = new Vector3<float>();
			if ( Keys.IsActive( Key.W ) )
				deltaPosition += Vector3<float>.UnitZ;
			if ( Keys.IsActive( Key.S ) )
				deltaPosition -= Vector3<float>.UnitZ;
			if ( Keys.IsActive( Key.D ) )
				deltaPosition += Vector3<float>.UnitX;
			if ( Keys.IsActive( Key.A ) )
				deltaPosition -= Vector3<float>.UnitX;

			deltaPosition = cameraRotation.Apply( deltaPosition );

			if ( Keys.IsActive( Key.Space ) )
				deltaPosition += Vector3<float>.UnitY;
			if ( Keys.IsActive( Key.LeftShift ) )
				deltaPosition -= Vector3<float>.UnitY;
			if ( deltaPosition != Vector3<float>.Zero ) {
				position += deltaPosition * deltaTime;
			}

			var cameraMatrix = Matrix4<float>.CreateTranslation( -position.X, -position.Y, -position.Z )
				* inverseCameraRotation;

			lastFrameTime = now;
			var time = (float)( now - startTime ).TotalSeconds;
			uniforms.Transfer( new Matrices() {
				Model = Matrix4<float>.FromAxisAngle( Vector3<float>.UnitX, -90f.Degrees() ),
				View = cameraMatrix,
				Projection = renderer.CreateLeftHandCorrectionMatrix<float>() 
					* Matrix4<float>.CreatePerspective( frame.Size.width, frame.Size.height, 0.1f, float.PositiveInfinity )
			} );
			commands.BindDescriptor( pipeline.Layout, pipeline.DescriptorSet );
			commands.DrawIndexed( (uint)model.Indices.Count );
			commands.FinishRenderPass();
			commands.Finish();

			commands.Submit( graphicsQueue, info.ImageAvailable, info.RenderFinished, info.InFlight );
			validateSwapchain( swapchain.Present( presentQueue, index, info.RenderFinished ), recreateSuboptimal: true );

			Sleep( 1 );
		}

		bool validateSwapchain ( VkResult result, bool recreateSuboptimal ) {
			if ( result >= 0 )
				return true;

			if ( (result == VkResult.ErrorOutOfDateKHR || (result == VkResult.SuboptimalKHR && recreateSuboptimal)) && !window.IsClosed ) {
				recreateSwapchain();
			}

			return false;
		}

		void recreateSwapchain () {
			device.WaitIdle();
			swapchain.Recreate( window.PixelSize );
		}

		protected override void Dispose ( bool disposing ) {
			window.Resized -= onWindowResized;
			if ( !IsInitialized )
				return;

			device.WaitIdle();

			frameInfo.Dispose();
			commandPool.Dispose();
			copyCommandPool.Dispose();
			pipeline.Dispose();
			renderPass.Dispose();
			fragment.Dispose();
			vertex.Dispose();
			swapchain.Dispose();
			vertexBuffer.Dispose();
			indexBuffer.Dispose();
			uniforms.Dispose();
			texture.Dispose();
			sampler.Dispose();
			device.Dispose();
			renderer.Dispose();
		}
	}
}