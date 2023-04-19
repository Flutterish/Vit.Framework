using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Windowing.Sdl;

class Direct3D11Window : SdlWindow {
	public Direct3D11Window ( SdlHost host ) : base( host, GraphicsApiType.Direct3D11 ) { }

	//public override (ISwapchain swapchain, IGraphicsDevice device) CreateSwapchain ( Renderer renderer ) {
	//	throw new NotImplementedException();
	//}
}