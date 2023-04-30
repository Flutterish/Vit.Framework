using Vit.Framework.Graphics.Curses;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Platform;

namespace Vit.Framework.Windowing.Console;

public class ConsoleHost : Host {
	public ConsoleHost ( App primaryApp ) : base( primaryApp ) {
		
	}

	public override Window CreateWindow ( GraphicsApiType renderingApi ) {
		return renderingApi switch {
			GraphicsApiType.Curses => new ConsoleWindow(),
			_ => throw new ArgumentException( $"Unsupported rendering api: {renderingApi}", nameof( renderingApi ) )
		};
	}

	public override GraphicsApi CreateGraphicsApi ( GraphicsApiType api, IEnumerable<RenderingCapabilities> capabilities ) {
		return api switch {
			GraphicsApiType.Curses => new CursesApi( capabilities ),
			_ => throw new ArgumentException( $"Unsupported rendering api: {api}", nameof( api ) )
		};
	}

	public override IEnumerable<GraphicsApiType> SupportedRenderingApis { get; } = new[] {
		GraphicsApiType.Curses
	};

	public override void Dispose ( bool isDisposing ) {
		throw new NotImplementedException();
	}
}
