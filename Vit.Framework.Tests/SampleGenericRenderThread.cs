using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Platform;
using Vit.Framework.Windowing;

namespace Vit.Framework.Tests;

public class SampleGenericRenderThread : GenericRenderThread {
	public SampleGenericRenderThread ( Window window, Host host, string name ) : base( window, host, name ) {
	}

	protected override void Initialize () {
		base.Initialize();
	}

	protected override void Render ( NativeFramebuffer framebuffer, ICommandBuffer commands ) {
		using var _ = commands.RenderTo( framebuffer, clearColor: new( 0.5f, 0, 0 ) );
	}
}
