using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Specialisation;

namespace Vit.Framework.Graphics.Curses;

public class CursesApi : GraphicsApi {
	public static readonly GraphicsApiType GraphicsApiType = new() {
		KnownName = null,
		Name = "Curses [Software]",
		Version = -1
	};

	public CursesApi ( IEnumerable<RenderingCapabilities> capabilities ) : base( GraphicsApiType, capabilities ) {
	}

	protected override void Dispose ( bool disposing ) {
		throw new NotImplementedException();
	}
}
