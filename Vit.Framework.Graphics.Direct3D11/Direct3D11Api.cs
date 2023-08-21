using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Textures;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Vit.Framework.Graphics.Direct3D11;

public class Direct3D11Api : GraphicsApi {
	public static readonly GraphicsApiType GraphicsApiType = new() {
		KnownName = KnownGraphicsApiName.Direct3D11,
		Name = "Direct 3D",
		Version = 11
	};
	public Direct3D11Api ( IEnumerable<RenderingCapabilities> capabilities ) : base( GraphicsApiType, capabilities ) {
		
	}

	protected override void Dispose ( bool disposing ) {
		throw new NotImplementedException();
	}

	public static Dictionary<PixelFormat, Format> formats = new() {
		[PixelFormat.Rgba8] = Format.R8G8B8A8_UNorm,
		[PixelFormat.D24] = Format.D24_UNorm_S8_UInt,
		[PixelFormat.D32f] = Format.D32_Float,
		[PixelFormat.D24S8ui] = Format.D24_UNorm_S8_UInt,
		[PixelFormat.D32fS8ui] = Format.D32_Float_S8X24_UInt
	};
}
