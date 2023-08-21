using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Textures;

public class SamplerState : DisposableObject, ISampler {
	public readonly ID3D11SamplerState Sampler;

	public SamplerState ( ID3D11Device device ) {
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
	}

	protected override void Dispose ( bool disposing ) {
		Sampler.Dispose();
	}
}
