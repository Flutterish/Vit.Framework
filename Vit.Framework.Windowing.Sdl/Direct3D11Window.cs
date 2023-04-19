using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;

namespace Vit.Framework.Windowing.Sdl;

class Direct3D11Window : SdlWindow {
	public Direct3D11Window ( SdlHost host ) : base( host, GraphicsApiType.Direct3D11 ) { }

	public override (NativeSwapchain swapchain, Renderer renderer) CreateSwapchain ( GraphicsApi api, SwapChainArgs args ) {
		throw new NotImplementedException();
	}
}