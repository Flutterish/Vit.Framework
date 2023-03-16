using SDL2;
using System.Collections.Concurrent;
using Vit.Framework.Platform;
using Vit.Framework.Windowing;

namespace Vit.Framework.SdlWindowing;

public class SdlHost : Host {
	internal ConcurrentQueue<Action> scheduledActions = new();
	Thread eventThread;
	public SdlHost () {
		eventThread = new( eventLoop ) { Name = "Sdl Event Thread" };
		eventThread.Start();
	}

	void eventLoop () {
		if ( SDL.SDL_Init( SDL.SDL_INIT_VIDEO ) < 0 ) {
			ThrowSdl( "sdl initialisation failed" );
		}

		isRunning = true;

		SDL.SDL_Event e;
		while ( !isQuitting ) {
			while ( scheduledActions.TryDequeue( out var action ) )
				action();

			if ( SDL.SDL_PollEvent( out e ) == 0 )
				Thread.Sleep( 1 );

			if ( e.type == SDL.SDL_EventType.SDL_WINDOWEVENT ) {
				var @event = e.window;
				if ( @event.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE ) {
					windowsById.Remove( @event.windowID, out var window );
					window?.Dispose();
				}
			}
		}

		isRunning = false;
	}

	Dictionary<uint, Window> windowsById = new();
	public override Window CreateWindow () {
		var window = new SdlWindow();
		scheduledActions.Enqueue( () => {
			window.Init();
			windowsById[window.Id] = window;
		} );
		return window;
	}

	bool isRunning;
	bool isQuitting;
	public override void Dispose () {
		isQuitting = true;
		while ( isRunning ) { };
		foreach ( var i in windowsById )
			i.Value.Dispose();
		SDL.SDL_Quit();
		GC.SuppressFinalize( this );
	}

	~SdlHost () {
		Dispose();
	}

	internal static void ThrowSdl ( string reason ) { // TODO handle this better than with exceptions
		throw new InvalidOperationException( $"SDL Error ({reason}): {SDL.SDL_GetError()}" );
	}
}
