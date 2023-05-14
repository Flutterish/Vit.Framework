using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.TwoD;
using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Graphics.TwoD.Rendering;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Platform;
using Vit.Framework.Threading;
using Vit.Framework.Windowing;
using Vit.Framework.Windowing.Sdl;

namespace Vit.Framework.Tests;

public class TwoDTestApp : App {
	public TwoDTestApp ( string name ) : base( name ) {
		
	}

	Host host = null!;
	Window window = null!;
	ViewportContainer<Drawable> root = null!;
	DrawableRenderer drawableRenderer = null!;
	protected override void Initialize () {
		host = new SdlHost( primaryApp: this );
		var api = host.SupportedRenderingApis.First( x => x.KnownName == KnownGraphicsApiName.OpenGl );
		window = host.CreateWindow( api );
		window.Title = $"New Window [{Name}] [{api}]";
		window.Initialized += _ => {
			root = new( (1920, 1080), window.Size.Cast<float>(), FillMode.Fit ) {
				Position = (-1, -1)
			};
			drawableRenderer = new( root );

			ThreadRunner.RegisterThread( new UpdateThread( drawableRenderer, window, $"Update Thread [{Name}]" ) );
			ThreadRunner.RegisterThread( new RenderThread( drawableRenderer, host, window, api, $"Render Thread [{Name}]" ) );
		};

		window.Closed += _ => {
			Quit();
		};
	}

	public class RenderThread : AppThread {
		DrawableRenderer drawableRenderer;
		GraphicsApi api;
		Window window;
		public RenderThread ( DrawableRenderer drawableRenderer, Host host, Window window, GraphicsApiType api, string name ) : base( name ) {
			this.api = host.CreateGraphicsApi( api, new[] { RenderingCapabilities.DrawToWindow } );
			this.window = window;
			this.drawableRenderer = drawableRenderer;

			window.Resized += onWindowResized;
		}

		ISwapchain swapchain = null!;
		IRenderer renderer = null!;
		protected override void Initialize () {
			(swapchain, renderer) = window.CreateSwapchain( api, new() { 
				Depth = DepthFormat.Bits24, 
				Stencil = StencilFormat.Bits8,
				Multisample = MultisampleFormat.Samples4
			} );
		}

		bool windowResized;
		void onWindowResized ( Window _ ) {
			windowResized = true;
		}

		protected override void Loop () {
			if ( windowResized ) { // BUG this can crash and is laggy
				windowResized = false;
				swapchain.Recreate();
			}

			if ( swapchain.GetNextFrame( out var index ) is not IFramebuffer frame )
				return;

			//shaderStore.CompileNew( commands.Renderer );
			var mat = Matrix3<float>.CreateViewport( 1, 1, window.Width / 2, window.Height / 2 ) * new Matrix3<float>( renderer.CreateLeftHandCorrectionMatrix<float>() );
			//globalUniformBuffer.Upload( new GlobalUniforms {
			//	Matrix = new( mat )
			//} );
			using ( var commands = swapchain.CreateImmediateCommandBufferForPresentation() ) {
				using var _ = commands.RenderTo( frame );
				commands.SetTopology( Topology.Triangles );
				commands.SetViewport( frame.Size );
				commands.SetScissors( frame.Size );

				drawableRenderer.Draw( commands );
			}
			swapchain.Present( index );
		}

		protected override void Dispose ( bool disposing ) {
			window.Resized -= onWindowResized;

			if ( !IsInitialized )
				return;

			renderer.WaitIdle();

			swapchain.Dispose();
			renderer.Dispose();
			api.Dispose();
		}
	}

	public class UpdateThread : AppThread {
		DrawableRenderer drawableRenderer;
		Window window;
		public UpdateThread ( DrawableRenderer drawableRenderer, Window window, string name ) : base( name ) {
			this.drawableRenderer = drawableRenderer;
			this.window = window;
		}

		protected override void Initialize () {
			
		}

		protected override void Loop () {
			((ViewportContainer<Drawable>)drawableRenderer.Root).AvailableSize = window.Size.Cast<float>();
			drawableRenderer.CollectDrawData();
		}
	}
}
