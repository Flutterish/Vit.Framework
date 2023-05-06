using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Platform;
using Vit.Framework.Tests.GraphicsApis;
using Vit.Framework.Threading;
using Vit.Framework.Windowing;
using Vit.Framework.Windowing.Console;
using Vit.Framework.Windowing.Sdl;

namespace Vit.Framework.Tests;

public partial class Program : App {
	public Program () : base( "Test App" ) { }

	public static void Main () {
		var app = new Program();
		app.ThreadRunner.ThreadingMode = ThreadingMode.Multithreaded;
		using var host = new SdlHost( app );
		app.Run( host );

		GC.Collect();
		Thread.Sleep( 1000 );
	}

	protected override void Initialize ( Host host ) {
		List<GraphicsApiType> apis = new() {
			//GraphicsApiType.Software, 
			GraphicsApiType.Direct3D11, 
			//GraphicsApiType.Vulkan,
			//GraphicsApiType.OpenGl
		};

		List<Window> windows = new();
		var Letters = "ABCD";
		for ( int i = 0; i < apis.Count; i++ ) {
			var api = apis[i];

			var windowHost = host;
			if ( api == GraphicsApiType.Software ) {
				windowHost = new ConsoleHost( this );
			}

			var window = windowHost.CreateWindow( api, this );
			window.Title = $"Window {Letters[i]} [{api}]";
			window.Initialized += _ => {
				var graphicsApi = windowHost.CreateGraphicsApi( api, new[] { RenderingCapabilities.DrawToWindow } );
				ThreadRunner.RegisterThread( new Test05_Depth( window, windowHost, window.Title, graphicsApi ) );
			};
			windows.Add( window );
		}

		Task.Run( async () => {
			while ( windows.Any( x => !x.IsClosed ) )
				await Task.Delay( 1 );

			Quit();
		} );
	}
}