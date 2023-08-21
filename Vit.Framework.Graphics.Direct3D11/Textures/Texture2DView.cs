using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Textures;

public class Texture2DView : DisposableObject, ITexture2DView {
	public ITexture2D Source { get; }
	public readonly ID3D11ShaderResourceView ResourceView;

	public Texture2DView ( ID3D11Device device, Texture2D texture ) {
		Source = texture;

		ResourceView = device.CreateShaderResourceView( texture.Texture );
	}

	protected override void Dispose ( bool disposing ) {
		ResourceView.Dispose();
	}
}
