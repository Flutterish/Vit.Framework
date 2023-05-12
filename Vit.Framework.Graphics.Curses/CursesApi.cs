using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Graphics.Curses;

public class CursesApi : GraphicsApi {
	public static readonly GraphicsApiType GraphicsApiType = new() {
		KnownName = KnownGraphicsApiName.Vulkan,
		Name = "Curses [Software]",
		Version = -1
	};

	public CursesApi ( IEnumerable<RenderingCapabilities> capabilities ) : base( GraphicsApiType, capabilities ) {
	}

	protected override void Dispose ( bool disposing ) {
		throw new NotImplementedException();
	}
}
