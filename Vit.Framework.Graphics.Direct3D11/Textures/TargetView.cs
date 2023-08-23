using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Textures;

public class TargetView : DisposableObject, IFramebuffer {
	public readonly ID3D11RenderTargetView[] ColorAttachments;
	public readonly ID3D11DepthStencilView? DepthStencil;
	public TargetView ( IEnumerable<ID3D11Resource> attachments, ID3D11Resource? depthStencil = null ) {
		var device = (depthStencil ?? attachments.First()).Device;
		
		if ( depthStencil != null )
			DepthStencil = device.CreateDepthStencilView( depthStencil );
		ColorAttachments = attachments.Select( x => device.CreateRenderTargetView( x ) ).ToArray();
	}

	protected override void Dispose ( bool disposing ) {
		DepthStencil?.Dispose();
		foreach ( var i in ColorAttachments ) {
			i.Dispose();
		}
	}
}
