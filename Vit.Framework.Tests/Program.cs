using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Platform;
using Vit.Framework.SdlWindowing;

namespace Vit.Framework.Tests;

public class Program : App {
	public static void Main () {
		using var host = new SdlHost();
		host.Run( new Program() );
	}

	public override void Initialize ( Host host ) {
		var a = host.CreateWindow( RenderingApi.OpenGl );
		a.Title = "Window A";
		var b = host.CreateWindow( RenderingApi.OpenGl );
		b.Title = "Window B";
		var c = host.CreateWindow( RenderingApi.OpenGl );
		c.Title = "Window C";

		Task.Run( async () => {
			while ( !a.IsClosed || !b.IsClosed || !c.IsClosed )
				await Task.Delay( 1 );

			host.Quit();
		} );
	}
}