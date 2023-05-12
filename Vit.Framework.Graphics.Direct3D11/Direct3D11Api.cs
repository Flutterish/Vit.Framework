using Vit.Framework.Graphics.Rendering;

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
}
