using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Platform;
using Vit.Framework.Windowing.Sdl;

namespace Vit.Framework.Tests;

public class Program : App {
	public Program () : base( "Test App" ) { }

	public static void Main () {
		var app = new Program();
		using var host = new SdlHost( app );
		app.Run( host );
	}

	protected override void Initialize ( Host host ) {
		var a = host.CreateWindow( RenderingApi.OpenGl, this );
		a.Title = "Window A";
		var b = host.CreateWindow( RenderingApi.OpenGl, this );
		b.Title = "Window B";
		var c = host.CreateWindow( RenderingApi.OpenGl, this );
		c.Title = "Window C";

		Task.Run( async () => {
			while ( !a.IsClosed || !b.IsClosed || !c.IsClosed )
				await Task.Delay( 1 );

			Quit();
		} );
	}
}