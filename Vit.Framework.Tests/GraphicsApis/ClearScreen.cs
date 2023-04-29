using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Platform;
using Vit.Framework.Windowing;

namespace Vit.Framework.Tests.GraphicsApis;

public class ClearScreen : GenericRenderThread {
	public ClearScreen ( Window window, Host host, string name ) : base( window, host, name ) {
	}

	protected override void Render ( IFramebuffer framebuffer, ICommandBuffer commands ) {
		using var _ = commands.RenderTo( framebuffer, clearColor: new( 0.5f, 0, 0 ), clearDepth: 1 );
	}
}
