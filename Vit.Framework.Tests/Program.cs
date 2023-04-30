using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Memory;
using Vit.Framework.Platform;
using Vit.Framework.Threading;
using Vit.Framework.Windowing.Sdl;

namespace Vit.Framework.Tests;

public partial class Program : App {
	public Program () : base( "Test App" ) { }

	class lol : DisposableObject {
		protected override void Dispose ( bool disposing ) {
			throw new NotImplementedException();
		}
	}

	public static void Main () {
		var app = new Program();
		app.ThreadRunner.ThreadingMode = ThreadingMode.Multithreaded;
		using var host = new SdlHost( app );
		app.Run( host );

		GC.Collect();
		Thread.Sleep( 1000 );
	}

	protected override void Initialize ( Host host ) {
		var a = host.CreateWindow( GraphicsApiType.Direct3D11, this );
		a.Title = "Window A [DX11]";
		a.Initialized += _ => {
			var glApi = host.CreateGraphicsApi( GraphicsApiType.Direct3D11, new[] { RenderingCapabilities.DrawToWindow } );
			ThreadRunner.RegisterThread( new SdlDirect3D11RenderThreadX( (SdlWindow)a, "d3d" ) );
			//ThreadRunner.RegisterThread( new ClearScreen( a, host, a.Title, glApi ) );
		};
		//var b = host.CreateWindow( GraphicsApiType.OpenGl, this );
		//b.Title = "Window B [OpenGL]";
		//b.Initialized += _ => {
		//	var glApi = host.CreateGraphicsApi( GraphicsApiType.OpenGl, new[] { RenderingCapabilities.DrawToWindow } );
		//	ThreadRunner.RegisterThread( new ClearScreen( b, host, b.Title, glApi ) );
		//};
		//var c = host.CreateWindow( RenderingApi.Vulkan, this );
		//c.Title = "Window C [Vulkan]";
		//c.Initialized += _ => {
		//	ThreadRunner.RegisterThread( new RenderThread( c, host, c.Title ) );
		//};

		Task.Run( async () => {
			while ( !a.IsClosed /*|| !b.IsClosed*/ /*|| !c.IsClosed*/ )
				await Task.Delay( 1 );

			Quit();
		} );
	}
}