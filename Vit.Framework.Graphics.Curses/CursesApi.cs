using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Graphics.Curses;

public class CursesApi : GraphicsApi {
	public CursesApi ( IEnumerable<RenderingCapabilities> capabilities ) : base( GraphicsApiType.Curses, capabilities ) {
	}

	protected override void Dispose ( bool disposing ) {
		throw new NotImplementedException();
	}
}
