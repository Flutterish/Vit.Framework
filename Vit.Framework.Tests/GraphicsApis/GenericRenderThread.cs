using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Platform;
using Vit.Framework.Threading;
using Vit.Framework.Windowing;

namespace Vit.Framework.Tests.GraphicsApis;

public abstract class GenericRenderThread : AppThread {
	protected readonly Host Host;
	protected readonly Window Window;

	public GenericRenderThread ( Window window, Host host, string name, GraphicsApi api ) : base( name ) {
		Host = host;
		Window = window;
		GraphicsApi = api;
		window.Resized += onWindowResized;
	}

	bool windowResized;
	void onWindowResized ( Window _ ) {
		windowResized = true;
	}

	protected GraphicsApi GraphicsApi;
	protected IRenderer Renderer = null!;
	protected ISwapchain Swapchain = null!;
	protected override void Initialize () {
		(Swapchain, Renderer) = Window.CreateSwapchain( GraphicsApi, new() {
			Multisample = new() { Ideal = MultisampleFormat.Samples8, Minimum = MultisampleFormat.None, Maximum = MultisampleFormat.Samples16 },
			Depth = new() { Ideal = DepthFormat.Bits32, Minimum = DepthFormat.Bits16 },
			Stencil = StencilFormat.Bits8
		} );
	}

	protected sealed override void Loop () {
		if ( windowResized ) { // BUG this can crash and is laggy
			windowResized = false;
			Swapchain.Recreate();
		}

		if ( Swapchain.GetNextFrame( out var frameIndex ) is not IFramebuffer fb ) {
			return;
		}

		using ( var commands = Swapchain.CreateImmediateCommandBufferForPresentation() ) {
			Render( fb, commands );
		}
		Swapchain.Present( frameIndex );
	}

	protected abstract void Render ( IFramebuffer framebuffer, ICommandBuffer commands );

	protected sealed override void Dispose ( bool disposing ) {
		Window.Resized -= onWindowResized;

		if ( !IsInitialized )
			return;

		Renderer.WaitIdle();

		Dispose();

		Swapchain.Dispose();
		Renderer.Dispose();
		GraphicsApi.Dispose();
	}

	new protected virtual void Dispose () { }
}
