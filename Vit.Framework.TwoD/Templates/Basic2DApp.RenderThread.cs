using System.Collections.Concurrent;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Pooling;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Platform;
using Vit.Framework.Threading;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.Windowing;

namespace Vit.Framework.TwoD.Templates;

public abstract partial class Basic2DApp<TRoot> {
	protected abstract class RenderThread : AppThread {
		DrawNodeRenderer drawNodeRenderer;
		RenderThreadScheduler disposeScheduler;
		protected GraphicsApi Api;
		protected Window Window;
		public readonly ConcurrentQueue<Action> Scheduler = new();
		protected TRoot Root => (TRoot)drawNodeRenderer.Root;
		protected readonly IReadOnlyDependencyCache Dependencies;
		public RenderThread ( DrawNodeRenderer drawNodeRenderer, RenderThreadScheduler disposeScheduler, Host host, Window window, GraphicsApiType api, IReadOnlyDependencyCache dependencies, string name ) : base( name ) {
			this.Api = host.CreateGraphicsApi( api, new[] { RenderingCapabilities.DrawToWindow } );
			this.Window = window;
			this.drawNodeRenderer = drawNodeRenderer;
			this.disposeScheduler = disposeScheduler;
			Dependencies = dependencies;

			window.Resized += onWindowResized;
		}

		Task<WindowGraphicsSurface>? initializationTask;
		protected WindowGraphicsSurface GraphicsSurface = null!;
		protected ISwapchain Swapchain = null!;
		protected IRenderer Renderer = null!;
		protected override bool Initialize () {
			initializationTask ??= Window.CreateGraphicsSurface( Api, new() {
				Depth = DepthFormat.Bits24,
				Stencil = StencilFormat.Bits8,
				Multisample = MultisampleFormat.Samples4
			} );

			if ( !initializationTask.IsCompleted )
				return false;

			GraphicsSurface = initializationTask.Result;
			(Swapchain, Renderer) = (GraphicsSurface.Swapchain, GraphicsSurface.Renderer);

			SingleUseBuffers = Dependencies.Resolve<SingleUseBufferSectionStack>();
			SingleUseBuffers.Initialize( Renderer );

			foreach ( var (id, dep) in ((DependencyCache)Dependencies).EnumerateCached() ) {
				if ( dep is IDrawDependency drawDependency )
					drawDependency.Initialize( Renderer, Dependencies );
			}

			ShaderStore = Dependencies.Resolve<ShaderStore>();
			TextureStore = Dependencies.Resolve<TextureStore>();

			ShaderStore.CompileNew( Renderer );
			TextureStore.UploadNew( Renderer );

			var updateThread = Dependencies.Resolve<UpdateThread>();
			updateThread.Scheduler.Enqueue( () => {
				updateThread.Renderer = Renderer;
			} );

			return true;
		}

		protected ShaderStore ShaderStore = null!;
		protected TextureStore TextureStore = null!;
		protected SingleUseBufferSectionStack SingleUseBuffers = null!;
		bool windowResized;
		void onWindowResized ( Window _ ) {
			windowResized = true;
		}

		protected sealed override void Loop () {
			while ( Scheduler.TryDequeue( out var action ) ) {
				action();
			}

			if ( windowResized ) { // BUG this can crash and is laggy
				windowResized = false;
				Swapchain.Recreate();
			}

			if ( Swapchain.GetNextFrame( out var index ) is not IFramebuffer frame )
				return;

			ShaderStore.CompileNew( Renderer );
			TextureStore.UploadNew( Renderer );

			BeforeRender();
			using ( var commands = Swapchain.CreateImmediateCommandBufferForPresentation() ) {
				using var _ = commands.RenderTo( frame );
				commands.ClearColor( ColorSRgba.Blue );
				commands.ClearDepth( 1 );
				commands.ClearStencil( 0 );

				commands.SetTopology( Topology.Triangles );
				commands.SetBlending( BlendState.PremultipliedAlpha );
				var size = Window.Size;
				commands.SetViewport( Swapchain.BackbufferSize );
				commands.SetScissors( Swapchain.BackbufferSize );

				drawNodeRenderer.Draw( commands, disposeScheduler.Execute );
			}
			Swapchain.Present( index );
			SingleUseBuffers.EndFrame();
		}

		protected abstract void BeforeRender ();

		protected sealed override void Dispose ( bool disposing ) {
			Window.Resized -= onWindowResized;
			DisposeManaged( disposing );

			if ( !IsInitialized )
				return;

			Renderer.WaitIdle();

			DisposeGraphics( disposing );
			disposeScheduler.DisposeAll();

			Swapchain.Dispose();
			Renderer.Dispose();
			Api.Dispose();
		}

		protected virtual void DisposeManaged ( bool disposing ) { }

		protected virtual void DisposeGraphics ( bool disposing ) { }
	}
}

public interface IDrawDependency {
	void Initialize ( IRenderer renderer, IReadOnlyDependencyCache dependencies );
}