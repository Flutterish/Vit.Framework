using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Platform;
using Vit.Framework.Tests.GraphicsApis;
using Vit.Framework.Threading;
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
		var consoleHost = new ConsoleHost( this );
		var d = consoleHost.CreateWindow( GraphicsApiType.Curses, this );
		d.Title = "Window A [Curses]";
		d.Initialized += _ => {
			var api = consoleHost.CreateGraphicsApi( GraphicsApiType.Curses, new[] { RenderingCapabilities.DrawToWindow } );
			ThreadRunner.RegisterThread( new Test03_Uniforms( d, consoleHost, d.Title, api ) );
		};
		var a = host.CreateWindow( GraphicsApiType.Direct3D11, this );
		a.Title = "Window A [DX11]";
		a.Initialized += _ => {
			var api = host.CreateGraphicsApi( GraphicsApiType.Direct3D11, new[] { RenderingCapabilities.DrawToWindow } );
			ThreadRunner.RegisterThread( new Test03_Uniforms( a, host, a.Title, api ) );
		};
		var b = host.CreateWindow( GraphicsApiType.Vulkan, this );
		b.Title = "Window A [Vulkan]";
		b.Initialized += _ => {
			var api = host.CreateGraphicsApi( GraphicsApiType.Vulkan, new[] { RenderingCapabilities.DrawToWindow } );
			ThreadRunner.RegisterThread( new Test03_Uniforms( b, host, b.Title, api ) );
		};
		var c = host.CreateWindow( GraphicsApiType.OpenGl, this );
		c.Title = "Window A [OpenGl]";
		c.Initialized += _ => {
			var api = host.CreateGraphicsApi( GraphicsApiType.OpenGl, new[] { RenderingCapabilities.DrawToWindow } );
			ThreadRunner.RegisterThread( new Test03_Uniforms( c, host, c.Title, api ) );
		};

		Task.Run( async () => {
			while ( !a.IsClosed || !b.IsClosed || !c.IsClosed || !d.IsClosed )
				await Task.Delay( 1 );

			Quit();
		} );
	}
}