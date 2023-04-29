using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Platform;
using Vit.Framework.Windowing;

namespace Vit.Framework.Tests.GraphicsApis;

public class ClearScreen : GenericRenderThread {
	public ClearScreen ( Window window, Host host, string name, GraphicsApi api ) : base( window, host, name, api ) {
	}

	protected override void Render ( IFramebuffer framebuffer, ICommandBuffer commands ) {
		using var _ = commands.RenderTo( framebuffer, clearColor: ColorRgba.HotPink, clearDepth: 1 );
	}
}
