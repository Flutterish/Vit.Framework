using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;

namespace Vit.Framework.Windowing.Sdl;

class Direct3D11Window : SdlWindow {
	public Direct3D11Window ( SdlHost host ) : base( host, RenderingApi.Direct3D11 ) { }

	public override (ISwapchain swapchain, IGraphicsDevice device) CreateSwapchain ( Renderer renderer ) {
		throw new NotImplementedException();
	}
}