using Vit.Framework.Graphics.Curses;
using Vit.Framework.Graphics.Direct3D11;
using Vit.Framework.Graphics.OpenGl;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Specialisation;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Memory;
using Vit.Framework.Platform;
using Vit.Framework.Tests.AudioApis;
using Vit.Framework.Tests.GraphicsApis;
using Vit.Framework.Threading;
using Vit.Framework.Windowing;
using Vit.Framework.Windowing.Console;
using Vit.Framework.Windowing.Sdl;

namespace Vit.Framework.Tests;

public partial class Program : App {
	public Program () : base( "Test App" ) { }

	public static void Main () {
		//var app = new TwoDTestApp( typeof( VisualTestRunner ) );
		var app = new Program();
		app.ThreadRunner.ThreadingMode = ThreadingMode.Multithreaded;
		app.Run();
		app = null;

		Console.WriteLine( "Performing GC check before quitting..." );
		GC.Collect( GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true );
		Thread.Sleep( 1_000 );
		GC.Collect( GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true );
		Thread.Sleep( 1_000 );
		DisposableObject.ValidateEverythingIsDisposed();
	}

	protected override void Initialize () {
		//initAudio();
		initGraphics();
	}

	async void initGraphics () {
		List<GraphicsApiType> apis = new() {
			//CursesApi.GraphicsApiType,
			Direct3D11Api.GraphicsApiType,
			VulkanApi.GraphicsApiType,
			OpenGlApi.GraphicsApiType
		};

		using Host host = new SdlHost( this );
		using ConsoleHost consoleHost = new ConsoleHost( this );
		List<Task<Window>> windowTasks = new();
		List<Window> windows = new();
		var Letters = "ABCD";
		for ( int i = 0; i < apis.Count; i++ ) {
			var api = apis[i];
			var windowHost = api == CursesApi.GraphicsApiType ? consoleHost : host;
			windowTasks.Add( windowHost.CreateWindow() );
		}

		await Task.WhenAll( windowTasks );
		for ( int i = 0; i < windowTasks.Count; i++ ) {
			var window = windowTasks[i].Result;
			var api = apis[i];
			var windowHost = api == CursesApi.GraphicsApiType ? consoleHost : host;

			window.Title = $"Window {Letters[i]} [{api}]";
			var graphicsApi = windowHost.CreateGraphicsApi( api, new[] { RenderingCapabilities.DrawToWindow } );
			ThreadRunner.RegisterThread( new Test06_Framebuffers( window, windowHost, window.Title, graphicsApi ) );
			windows.Add( window );
		}

		while ( windows.Any( x => !x.IsClosed ) )
			await Task.Delay( 1 );

		Quit();
	}

	void initAudio () {
		ThreadRunner.RegisterThread( new Test01_Samples( "bass" ) );
	}
}