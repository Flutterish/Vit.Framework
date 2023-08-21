using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Textures;

public class Texture2D : DisposableObject, ITexture2D {
	public Size2<uint> Size { get; }
	public PixelFormat Format { get; }
	public readonly ID3D11Texture2D Texture;
	public readonly ID3D11SamplerState Sampler;
	public readonly ID3D11ShaderResourceView ResourceView;
	public Texture2D ( ID3D11Device device, Size2<uint> size, PixelFormat format ) {
		Debug.Assert( format == PixelFormat.Rgba8 );
		Size = size;
		Format = format;

		Texture = device.CreateTexture2D( new Texture2DDescription {
			Width = (int)size.Width,
			Height = (int)size.Height,
			MipLevels = 1,
			ArraySize = 1,
			Format = Vortice.DXGI.Format.R8G8B8A8_UNorm_SRgb,
			SampleDescription = {
				Count = 1,
				Quality = 0
			},
			Usage = ResourceUsage.Dynamic,
			BindFlags = BindFlags.ShaderResource,
			CPUAccessFlags = CpuAccessFlags.Write
		} );

		Sampler = device.CreateSamplerState( new() {
			Filter = Filter.MinMagMipLinear,
			AddressU = TextureAddressMode.Clamp,
			AddressV = TextureAddressMode.Clamp,
			AddressW = TextureAddressMode.Clamp,
			ComparisonFunc = ComparisonFunction.Never,
			MinLOD = 0,
			MaxLOD = 0,
			BorderColor = new( 1f, 1f, 1f, 1f )
		} );

		ResourceView = device.CreateShaderResourceView( Texture );
	}

	public void Upload<TPixel> ( ReadOnlySpan<TPixel> data, ID3D11DeviceContext context ) where TPixel : unmanaged {
		var map = context.Map<TPixel>( Texture, 0, 0, MapMode.WriteDiscard );
		data.CopyTo( map );
		context.Unmap( Texture, 0 );
	}

	protected override void Dispose ( bool disposing ) {
		ResourceView.Dispose();
		Sampler.Dispose();
		Texture.Dispose();
	}
}
