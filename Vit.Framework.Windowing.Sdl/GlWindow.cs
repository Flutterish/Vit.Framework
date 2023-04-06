using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Windowing.Sdl;

class GlWindow : SdlWindow {
	public GlWindow ( SdlHost host ) : base( host, RenderingApi.OpenGl ) { }

	//public override (ISwapchain swapchain, IGraphicsDevice device) CreateSwapchain ( Renderer renderer ) {
	//	throw new NotImplementedException();
	//}
}
