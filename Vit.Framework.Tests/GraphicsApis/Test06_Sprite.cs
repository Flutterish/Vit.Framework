using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.TwoD;
using Vit.Framework.Graphics.TwoD.Rendering;
using Vit.Framework.Platform;
using Vit.Framework.Windowing;

namespace Vit.Framework.Tests.GraphicsApis;

public class Test06_Sprite : GenericRenderThread {
	DrawableRenderer drawableRenderer;
	Sprite sprite;
	public Test06_Sprite ( Window window, Host host, string name, GraphicsApi api ) : base( window, host, name, api ) {
		sprite = new();
		drawableRenderer = new( sprite );
	}

	protected override void Render ( IFramebuffer framebuffer, ICommandBuffer commands ) {
		using var _ = commands.RenderTo( framebuffer );
		commands.SetTopology( Topology.Triangles );
		commands.SetViewport( framebuffer.Size );
		commands.SetScissors( framebuffer.Size );

		drawableRenderer.CollectDrawData();
		drawableRenderer.Draw( commands );
	}

	protected override void Dispose () {
		sprite.Dispose();
	}
}
