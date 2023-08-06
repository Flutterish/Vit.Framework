using Vit.Framework.Graphics.Curses.Queues;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Windowing;

namespace Vit.Framework.Graphics.Curses.Windowing;

public class CursesWindowSurface : WindowGraphicsSurface {
	IWindow window;
	public CursesWindowSurface ( CursesApi graphicsApi, WindowSurfaceArgs args, IWindow window ) : base( graphicsApi, args ) {
		this.window = window;
	}

	protected override (ISwapchain swapchain, IRenderer renderer) CreateSwapchain () {
		var renderer = new CursesRenderer( (CursesApi)GraphicsApi );
		return (new Swapchain( renderer, window ), renderer);
	}
}
