using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Textures;

public class TargetView : DisposableObject, IFramebuffer {
	public readonly ID3D11RenderTargetView Handle;
	readonly ID3D11Resource? depthStencilResource;
	public readonly ID3D11DepthStencilView? DepthStencil;
	public TargetView ( ID3D11RenderTargetView handle, (ID3D11Resource, ID3D11DepthStencilView)? depthStencil = null ) {
		Handle = handle;
		if ( depthStencil != null )
			(depthStencilResource, DepthStencil) = depthStencil.Value;
	}

	public Size2<uint> Size { get; set; }

	protected override void Dispose ( bool disposing ) {
		DepthStencil?.Dispose();
		depthStencilResource?.Dispose();

		Handle.Dispose();
	}
}
