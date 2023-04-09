using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Graphics.Vulkan.Buffers;
using Vit.Framework.Graphics.Vulkan.Queues;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vit.Framework.Graphics.Vulkan.Shaders;
using Vit.Framework.Graphics.Vulkan.Synchronisation;
using Vit.Framework.Graphics.Vulkan.Windowing;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Platform;
using Vit.Framework.Threading;
using Vit.Framework.Windowing;
using Vit.Framework.Windowing.Sdl;
using Vulkan;
using Semaphore = Vit.Framework.Graphics.Vulkan.Synchronisation.Semaphore;

namespace Vit.Framework.Tests;

public class Program : App {
	public Program () : base( "Test App" ) { }

	public static void Main () {
		//var unitVector = new Matrix<MultiVector<float>>( new MultiVector<float>[,] {
		//	{ new BasisVector<float>( "X" ), new BasisVector<float>( "Y" ), new BasisVector<float>( "Z" ), new SimpleBlade<float>( 1, Array.Empty<BasisVector<float>>() ) }
		//} );
		//var labelMatrix = Matrix<float>.GenerateLabelMatrix( "m", 4, 4 );
		//var help = ( unitVector * labelMatrix ).ToString();

		var app = new Program();
		app.ThreadRunner.ThreadingMode = ThreadingMode.Multithreaded;
		using var host = new SdlHost( app );
		app.Run( host );
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
		public RenderThread ( Window window, Host host, string name ) : base( name ) {
			startTime = DateTime.Now;
			this.host = host;
			this.window = window;
			renderer = (VulkanRenderer)host.CreateRenderer( RenderingApi.Vulkan, new[] { RenderingCapabilities.DrawToWindow }, new() {
				Window = window
			} )!;
			vulkan = renderer.Instance;

			window.Resized += onWindowResized;
		}

		bool windowResized;
		void onWindowResized ( Window _ ) {
			windowResized = true;
		}

		Device device = null!;
		SwapchainInfo swapchainInfo = null!;
		Swapchain swapchain = null!;
		ShaderModule vertex = null!;
		ShaderModule fragment = null!;
		RenderPass renderPass = null!;
		Pipeline pipeline = null!;
		CommandPool commandPool = null!;
		CommandPool copyCommandPool = null!;
		DeviceBuffer<float> vertexBuffer = null!;
		DeviceBuffer<ushort> indexBuffer = null!;
		struct Matrices {
			public Matrix4<float> Model;
			public Matrix4<float> View;
			public Matrix4<float> Projection;
		}
		HostBuffer<Matrices> uniforms = null!;
		VkClearColorValue bg;

		VkQueue graphicsQueue;
		VkQueue presentQueue;
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
				layout(location = 0) in vec2 inPosition;
				layout(location = 1) in vec3 inColor;

				layout(location = 0) out vec3 fragColor;

				layout(binding = 0) uniform Matrices {
					mat4 model;
					mat4 view;
					mat4 projection;
				} matrices;

				void main() {
					gl_Position = matrices.projection * matrices.view * matrices.model * vec4(inPosition, 0.0, 1.0);
					fragColor = inColor;
				}
			", ShaderLanguage.GLSL, ShaderPartType.Vertex ) );

			fragment = device.CreateShaderModule( new SpirvBytecode( @"#version 450
				layout(location = 0) in vec3 fragColor;

				layout(location = 0) out vec4 outColor;

				void main() {
					outColor = vec4(fragColor, 1.0);
				}
			", ShaderLanguage.GLSL, ShaderPartType.Fragment ) );

			renderPass = new RenderPass( device, swapchain.Format.Format.format );
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

			vertexBuffer = new( device, VkBufferUsageFlags.VertexBuffer );
			vertexBuffer.AllocateAndTransfer( new float[] {
				-0.5f, -0.5f, 1, 0, 0,
				 0.5f, -0.5f, 0, 1, 0,
				 0.5f,  0.5f, 0, 0, 1,
				-0.5f,  0.5f, 1, 1, 1
			}, copyCommandPool, graphicsQueue );

			indexBuffer = new( device, VkBufferUsageFlags.IndexBuffer );
			indexBuffer.AllocateAndTransfer( new ushort[] {
				 0, 1, 2,
				 2, 3, 0
			}, copyCommandPool, graphicsQueue );

			uniforms = new( device, VkBufferUsageFlags.UniformBuffer );
			uniforms.Allocate( 1 );

			pipeline.DescriptorSet.ConfigureUniforms( uniforms );
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
		void record ( FrameInfo info ) {
			var commands = info.Commands;
			info.InFlight.Wait();

			if ( !validateSwapchain( swapchain.GetNextFrame( info.ImageAvailable, out var frame, out var index ), recreateSuboptimal: false ) ) {
				return;
			}

			info.InFlight.Reset();

			commands.Reset();
			commands.Begin();
			commands.BeginRenderPass( frame, new VkClearValue { color = bg } );
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
			var time = (float)( DateTime.Now - startTime ).TotalSeconds;
			var axis = Vector3<float>.UnitZ.Lerp( Vector3<float>.UnitY, 0f );
			Matrices matrices;
			uniforms.Transfer( matrices = new Matrices() {
				Model = Matrix4<float>.FromAxisAngle( axis.Normalized(), time * 90f.Degrees() ), // CreateLookAt( Vector3<float>.Zero, Vector3<float>.UnitZ + Vector3<float>.UnitY, Vector3<float>.UnitY )
				View = Matrix4<float>.CreateTranslation( 0.1f, 0.1f, 2 ),
				Projection = Matrix4<float>.CreatePerspective( frame.Size.width, frame.Size.height, 0.1f, float.PositiveInfinity )
					* renderer.CreateLeftHandCorrectionMatrix<float>()
			} );
			commands.BindDescriptor( pipeline.Layout, pipeline.DescriptorSet );
			commands.DrawIndexed( 6 );
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
			device.Dispose();
			renderer.Dispose();
		}
	}
}