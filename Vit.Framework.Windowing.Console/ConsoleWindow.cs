using Vit.Framework.Graphics.Curses;
using Vit.Framework.Graphics.Curses.Windowing;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Input;
using Vit.Framework.Mathematics;
using Vit.Framework.Windowing.Console.Input;

namespace Vit.Framework.Windowing.Console;

public class ConsoleWindow : Window {
	public ConsoleWindow () { }

	public override string Title {
		get => System.Console.Title ?? string.Empty;
		set => System.Console.Title = value;
	}
	public override Size2<uint> Size {
		get => new( (uint)System.Console.WindowWidth, (uint)System.Console.WindowHeight * 2 );
		set => System.Console.SetWindowSize( (int)value.Width, (int)value.Height / 2 );
	}

	public override Size2<uint> PixelSize => new( (uint)System.Console.WindowWidth, (uint)System.Console.WindowHeight );

	public override Task<WindowGraphicsSurface> CreateGraphicsSurface ( GraphicsApi api, WindowSurfaceArgs args ) {
		if ( api is not CursesApi curses )
			throw new ArgumentException( "Graphics API must be a Curses API created from the same host as this window", nameof( api ) );

		return Task.FromResult<WindowGraphicsSurface>( new CursesWindowSurface( curses, args, this ) );
	}

	public override InputTrackerCollection CreateInputTrackers () {
		return new ConsoleInputTrackerCollection();
	}

	protected override void Dispose ( bool disposing ) {
		throw new NotImplementedException();
	}
}