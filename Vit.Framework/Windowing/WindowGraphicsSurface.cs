using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Rendering.Textures;

namespace Vit.Framework.Windowing;

/// <summary>
/// A graphics-api specific representation of a window.
/// </summary>
/// <remarks>
/// Its properties and methods should only be used on the appropriate render thread.
/// </remarks>
public abstract class WindowGraphicsSurface {
	public readonly GraphicsApi GraphicsApi;
	public readonly WindowSurfaceArgs Args;
	public WindowGraphicsSurface ( GraphicsApi graphicsApi, WindowSurfaceArgs args ) {
		GraphicsApi = graphicsApi;
		Args = args;
	}

	// TODO try to split this to just create the swapchain?
	/// <summary>
	/// Creates a swapchain and a renderer for it. 
	/// </summary>
	protected abstract (ISwapchain swapchain, IRenderer renderer) CreateSwapchain ();

	ISwapchain? swapchain;
	public ISwapchain Swapchain {
		get {
			if ( swapchain == null ) {
				(swapchain, renderer) = CreateSwapchain();
			}
			return swapchain;
		}
	}

	IRenderer? renderer;
	public IRenderer Renderer {
		get {
			if ( renderer == null ) {
				(swapchain, renderer) = CreateSwapchain();
			}
			return renderer;
		}
	}
}

public struct WindowSurfaceArgs {
	public AcceptableRange<MultisampleFormat> Multisample;
	public AcceptableRange<DepthFormat> Depth;
	public AcceptableRange<StencilFormat> Stencil;
}
