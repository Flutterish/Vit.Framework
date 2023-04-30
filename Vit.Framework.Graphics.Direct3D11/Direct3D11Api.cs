using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Graphics.Direct3D11;

public class Direct3D11Api : GraphicsApi {
	public Direct3D11Api ( IEnumerable<RenderingCapabilities> capabilities ) : base( GraphicsApiType.Direct3D11, capabilities ) {

	}

	protected override void Dispose ( bool disposing ) {
		throw new NotImplementedException();
	}
}
