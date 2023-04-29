using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Platform;
using Vit.Framework.Tests.GraphicsApis;
using Vit.Framework.Threading;
using Vit.Framework.Windowing.Sdl;

namespace Vit.Framework.Tests;

public partial class Program : App {
	public Program () : base( "Test App" ) { }

	public static void Main () {
		var app = new Program();
		app.ThreadRunner.ThreadingMode = ThreadingMode.Multithreaded;
		using var host = new SdlHost( app );
		app.Run( host );
	}

	protected override void Initialize ( Host host ) {

		var a = host.CreateWindow( GraphicsApiType.OpenGl, this );
		a.Title = "Window A [OpenGL]";
		a.Initialized += _ => {
			var glApi = host.CreateGraphicsApi( GraphicsApiType.OpenGl, new[] { RenderingCapabilities.DrawToWindow } );
			ThreadRunner.RegisterThread( new ClearScreen( a, host, a.Title, glApi ) );
		};
		//var b = host.CreateWindow( RenderingApi.Vulkan, this );
		//b.Title = "Window B [Vulkan]";
		//b.Initialized += _ => {
		//	ThreadRunner.RegisterThread( new RenderThread( b, host, b.Title ) );
		//};
		//var c = host.CreateWindow( RenderingApi.Vulkan, this );
		//c.Title = "Window C [Vulkan]";
		//c.Initialized += _ => {
		//	ThreadRunner.RegisterThread( new RenderThread( c, host, c.Title ) );
		//};

		Task.Run( async () => {
			while ( !a.IsClosed /*|| !b.IsClosed || !c.IsClosed*/ )
				await Task.Delay( 1 );

			Quit();
		} );
	}
}