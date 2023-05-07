using Vit.Framework.Graphics.Curses;
using Vit.Framework.Graphics.Curses.Queues;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Windowing.Console;

public class ConsoleWindow : Window {
	public ConsoleWindow () : base( GraphicsApiType.Software ) {
		OnInitialized();
	}

	public override string Title {
		get => System.Console.Title ?? string.Empty;
		set => System.Console.Title = value;
	}
	public override Size2<uint> Size {
		get => new( (uint)System.Console.WindowWidth, (uint)System.Console.WindowHeight );
		set => System.Console.SetWindowSize( (int)value.Width, (int)value.Height );
	}

	public override (ISwapchain swapchain, IRenderer renderer) CreateSwapchain ( GraphicsApi api, SwapChainArgs args ) {
		if ( api is not CursesApi curses )
			throw new ArgumentException( "Graphics API must be a Curses API created from the same host as this window", nameof( api ) );

		var renderer = new CursesRenderer( curses );
		return (new Swapchain( renderer, this ), renderer);
	}

	protected override void Dispose ( bool disposing ) {
		throw new NotImplementedException();
	}
}
