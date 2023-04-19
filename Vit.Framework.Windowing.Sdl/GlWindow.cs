using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Windowing.Sdl;

class GlWindow : SdlWindow {
	public GlWindow ( SdlHost host ) : base( host, GraphicsApiType.OpenGl ) { }

	//public override (ISwapchain swapchain, IGraphicsDevice device) CreateSwapchain ( Renderer renderer ) {
	//	throw new NotImplementedException();
	//}
}
