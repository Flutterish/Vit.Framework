using Vit.Framework.Graphics.OpenGl.Queues;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Windowing;

namespace Vit.Framework.Graphics.OpenGl.Windowing;

public class GlWindowSurface : WindowGraphicsSurface {
	IGlWindow window;
	public GlWindowSurface ( OpenGlApi graphicsApi, WindowSurfaceArgs args, IGlWindow window ) : base( graphicsApi, args ) {
		this.window = window;
	}

	protected override (ISwapchain swapchain, IRenderer renderer) CreateSwapchain () {
		var renderer = new GlRenderer( (OpenGlApi)GraphicsApi );
		var swapchain = new GlSwapchain( renderer, window );

		return (swapchain, renderer);
	}
}
