using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Synchronisation;
using Vit.Framework.Platform;
using Vit.Framework.Threading;
using Vit.Framework.Windowing;
using Vit.Framework.Windowing.Sdl;

namespace Vit.Framework.Tests;

public class Program : App {
	public Program () : base( "Test App" ) { }

	public static void Main () {
		var app = new Program();
		app.ThreadRunner.ThreadingMode = Threading.ThreadingMode.Multithreaded;
		using var host = new SdlHost( app );
		app.Run( host );
	}

	protected override void Initialize ( Host host ) {
		var a = host.CreateWindow( RenderingApi.Vulkan, this );
		a.Title = "Window A [Vulkan]";
		a.Initialized += _ => {
			ThreadRunner.RegisterThread( new RenderThread( a, host, "Generic Render Thread" ) );
		};

		//var b = host.CreateWindow( RenderingApi.OpenGl, this );
		//b.Title = "Window B [OpenGL]";
		//var c = host.CreateWindow( RenderingApi.Direct3D11, this );
		//c.Title = "Window C [DX11]";

		Task.Run( async () => {
			while ( !a.IsClosed /*|| !b.IsClosed || !c.IsClosed*/ )
				await Task.Delay( 1 );

			Quit();
		} );
	}

	class RenderThread : AppThread {
		Window window;
		Host host;
		public RenderThread ( Window window, Host host, string name ) : base( name ) {
			if ( !window.IsInitialized )
				throw new ArgumentException( "Cannot create a renderer for an uninitialized window" );
			
			this.window = window;
			this.host = host;
		}

		Renderer renderer = null!;
		ISwapchain swapchain = null!;
		IGraphicsDevice device = null!;
		IShaderPart vertexShader = null!;
		IShaderPart fragmentShader = null!;
		IShader shader = null!;
		FrameData[] frameData = null!;
		struct FrameData {
			public IGpuBarrier ImageAvailable;
			public ICpuBarrier FrameRendered;
		}
		protected override void Initialize () {
			renderer = host.CreateRenderer( window.RenderingApi, new[] { RenderingCapabilities.DrawToWindow }, new() {
				Window = window
			} );
			(swapchain, device) = window.CreateSwapchain( renderer );
			frameData = Enumerable.Range( 0, 1 ).Select( x => new FrameData() {
				ImageAvailable = device.CreateGpuBarrier(),
				FrameRendered = device.CreateCpuBarrier( signaled: true )
			} ).ToArray();

			vertexShader = device.CreateShaderPart( new SpirvBytecode( @"#version 450
				layout(location = 0) out vec3 fragColor;

				vec2 positions[3] = vec2[](
					vec2(0.0, -0.5),
					vec2(0.5, 0.5),
					vec2(-0.5, 0.5)
				);

				vec3 colors[3] = vec3[](
					vec3(1.0, 0.0, 0.0),
					vec3(0.0, 1.0, 0.0),
					vec3(0.0, 0.0, 1.0)
				);

				void main() {
					gl_Position = vec4(positions[gl_VertexIndex], 0.0, 1.0);
					fragColor = colors[gl_VertexIndex];
				}", ShaderLanguage.GLSL, ShaderPartType.Vertex 
			) );
			fragmentShader = device.CreateShaderPart( new SpirvBytecode( @"#version 450
				layout(location = 0) in vec3 fragColor;
				layout(location = 0) out vec4 outColor;

				void main() {
					outColor = vec4(fragColor, 1.0);
				}", ShaderLanguage.GLSL, ShaderPartType.Fragment 
			) );
			shader = device.CreateShader( new[] { fragmentShader, vertexShader } );
			graphicsBuffer = swapchain.GraphicsQueue.CreateCommandBuffer();
		}

		ICommandBuffer graphicsBuffer = null!;
		int frameIndex = -1;
		protected override void Loop () {
			frameIndex = ( frameIndex + 1 ) % frameData.Length;
			var sync = frameData[frameIndex];
			sync.FrameRendered.Wait();
			sync.FrameRendered.Reset();

			var (buffer, index) = swapchain.GetNextFrameBuffer( sync.ImageAvailable );
			Sleep( 1 );
		}

		protected override void Dispose ( bool disposing ) {
			shader.Dispose();
			vertexShader.Dispose();
			fragmentShader.Dispose();
			foreach ( var i in frameData ) {
				i.ImageAvailable.Dispose();
			}
			swapchain.Dispose();
			device.Dispose();
			renderer.Dispose();
		}
	}
}