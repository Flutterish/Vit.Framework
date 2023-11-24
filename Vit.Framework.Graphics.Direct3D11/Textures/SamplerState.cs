using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Textures;

public class SamplerState : DisposableObject, ISampler {
	public readonly ID3D11SamplerState Sampler;

	public SamplerState ( ID3D11Device device, Graphics.Rendering.Textures.SamplerDescription description ) {
		static TextureAddressMode addressMode ( TextureWrapMode mode ) {
			return mode switch {
				TextureWrapMode.Repeat => TextureAddressMode.Wrap,
				TextureWrapMode.MirroredRepeat => TextureAddressMode.Mirror,
				TextureWrapMode.ClampToEdge => TextureAddressMode.Clamp,
				TextureWrapMode.TransparentBlackBorder or _ => TextureAddressMode.Border
			};
		}

        Vortice.Direct3D11.SamplerDescription info = new() {
			Filter = (description.EnableAnisotropy, description.MinificationFilter, description.MagnificationFilter, description.MipmapMode) switch {
				(true, _, _, _) => Filter.Anisotropic,
				(_, FilteringMode.Nearest, FilteringMode.Nearest, MipmapMode.None or MipmapMode.Nearest) => Filter.MinMagMipPoint,
				(_, FilteringMode.Nearest, FilteringMode.Nearest, MipmapMode.Linear) => Filter.MinMagPointMipLinear,
				(_, FilteringMode.Nearest, FilteringMode.Linear, MipmapMode.None or MipmapMode.Nearest) => Filter.MinPointMagLinearMipPoint,
				(_, FilteringMode.Nearest, FilteringMode.Linear, MipmapMode.Linear) => Filter.MinPointMagMipLinear,
				(_, FilteringMode.Linear, FilteringMode.Nearest, MipmapMode.None or MipmapMode.Nearest) => Filter.MinLinearMagMipPoint,
				(_, FilteringMode.Linear, FilteringMode.Nearest, MipmapMode.Linear) => Filter.MinLinearMagPointMipLinear,
				(_, FilteringMode.Linear, FilteringMode.Linear, MipmapMode.None or MipmapMode.Nearest) => Filter.MinMagLinearMipPoint,
				(_, FilteringMode.Linear, FilteringMode.Linear, MipmapMode.Linear) or _ => Filter.MinMagMipLinear,
			},
			AddressU = addressMode( description.WrapU ),
			AddressV = addressMode( description.WrapV ),
			AddressW = TextureAddressMode.Wrap,
			MaxAnisotropy = (int)description.MaximumAnisotropicFiltering,
			MipLODBias = description.MipmapLevelBias,
			MinLOD = description.MinimimMipmapLevel,
			MaxLOD = description.MaximimMipmapLevel,
			BorderColor = new( 0f, 0f, 0f, 0f )
		};

		Sampler = device.CreateSamplerState( info );
	}

	protected override void Dispose ( bool disposing ) {
		Sampler.Dispose();
	}
}
