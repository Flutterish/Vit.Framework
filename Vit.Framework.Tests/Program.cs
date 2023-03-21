using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Platform;
using Vit.Framework.Windowing.Sdl;

namespace Vit.Framework.Tests;

public class Program : App {
	public Program () : base( "Test App" ) { }

	public static void Main () {
		var app = new Program();
		app.ThreadRunner.ThreadingMode = Threading.ThreadingMode.Multithreaded;
		using var host = new SdlHost( app );
		app.Run( host );
	}

	protected override void Initialize ( Host host ) {
		var a = host.CreateWindow( RenderingApi.Vulkan, this );
		a.Title = "Window A [Vulkan]";
		var b = host.CreateWindow( RenderingApi.OpenGl, this );
		b.Title = "Window B [OpenGL]";
		var c = host.CreateWindow( RenderingApi.Vulkan, this );
		c.Title = "Window C [Vulkan]";

		Task.Run( async () => {
			while ( !a.IsClosed || !b.IsClosed || !c.IsClosed )
				await Task.Delay( 1 );

			Quit();
		} );
	}
}