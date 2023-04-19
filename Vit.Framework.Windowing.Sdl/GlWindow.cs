using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;

namespace Vit.Framework.Windowing.Sdl;

class GlWindow : SdlWindow {
	public GlWindow ( SdlHost host ) : base( host, GraphicsApiType.OpenGl ) { }

	public override (NativeSwapchain swapchain, Renderer renderer) CreateSwapchain ( GraphicsApi api, SwapChainArgs args ) {
		throw new NotImplementedException();
	}
}
