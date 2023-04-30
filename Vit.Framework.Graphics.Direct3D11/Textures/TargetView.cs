using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Textures;

public class TargetView : DisposableObject, IFramebuffer {
	public readonly ID3D11RenderTargetView Handle;
	public TargetView ( ID3D11RenderTargetView handle ) {
		Handle = handle;
	}

	public Size2<uint> Size { get; set; }

	protected override void Dispose ( bool disposing ) {
		Handle.Dispose();
	}
}
