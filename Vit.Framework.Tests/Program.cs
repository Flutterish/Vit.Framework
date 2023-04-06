using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Graphics.Vulkan.Queues;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vit.Framework.Graphics.Vulkan.Shaders;
using Vit.Framework.Graphics.Vulkan.Synchronisation;
using Vit.Framework.Graphics.Vulkan.Windowing;
using Vit.Framework.Interop;
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
		public RenderThread ( Window window, Host host, string name ) : base( name ) {
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
		VkClearColorValue bg;
		protected override void Initialize () {
			var surface = ( (IVulkanWindow)window ).GetSurface( vulkan );
			var (physicalDevice, info) = vulkan.GetBestDeviceInfo( surface );
			swapchainInfo = info;

			device = physicalDevice.CreateDevice( SwapchainInfo.RequiredExtensionsCstr, new CString[] { }, new[] {
				info.GraphicsQueue,
				info.PresentQueue
			} );
			swapchain = device.CreateSwapchain( surface, info.SelectBest(), window.PixelSize );

			vertex = device.CreateShaderModule( new SpirvBytecode( @"#version 450
				vec2 positions[3] = vec2[](
					vec2(0.0, -0.5),
					vec2(0.5, 0.5),
					vec2(-0.5, 0.5)
				);

				layout(location = 0) out vec3 fragColor;
				vec3 colors[3] = vec3[](
					vec3(1.0, 0.0, 0.0),
					vec3(0.0, 1.0, 0.0),
					vec3(0.0, 0.0, 1.0)
				);

				void main() {
					gl_Position = vec4(positions[gl_VertexIndex], 0.0, 1.0);
					fragColor = colors[gl_VertexIndex];
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
			frameInfos = new FrameInfo[2];
			foreach ( ref var frame in frameInfos.AsSpan() ) {
				frame = new() {
					ImageAvailable = device.CreateSemaphore(),
					RenderFinished = device.CreateSemaphore(),
					InFlight = device.CreateFence( signaled: true ),
					Commands = commandPool.CreateCommandBuffer()
				};
			}

			var rng = new Random();
			bg = new( rng.NextSingle(), rng.NextSingle(), rng.NextSingle() );
		}

		FrameInfo[] frameInfos = Array.Empty<FrameInfo>();
		int frameIndex = -1;
		protected override void Loop () {
			if ( windowResized ) {
				windowResized = false;
				recreateSwapchain();
			}

			frameIndex = ( frameIndex + 1 ) % frameInfos.Length;
			record( frameInfos[frameIndex] );
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
			commands.Bind( pipeline );
			commands.SetViewPort( new() {
				minDepth = 0,
				maxDepth = 1,
				width = frame.Size.width,
				height = frame.Size.height
			} );
			commands.SetScissor( new() {
				extent = frame.Size
			} );
			commands.Draw( 3 );
			commands.FinishRenderPass();
			commands.Finish();

			commands.Submit( device.GetQueue( swapchainInfo.GraphicsQueue ), info.ImageAvailable, info.RenderFinished, info.InFlight );
			validateSwapchain( swapchain.Present( device.GetQueue( swapchainInfo.PresentQueue ), index, info.RenderFinished ), recreateSuboptimal: true );

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

			foreach ( var i in frameInfos )
				i.Dispose();
			commandPool.Dispose();
			pipeline.Dispose();
			renderPass.Dispose();
			fragment.Dispose();
			vertex.Dispose();
			swapchain.Dispose();
			device.Dispose();
			renderer.Dispose();
		}
	}
}