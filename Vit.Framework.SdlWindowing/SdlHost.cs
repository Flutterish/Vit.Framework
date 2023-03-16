using SDL2;
using System.Collections.Concurrent;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Platform;
using Vit.Framework.Windowing;

namespace Vit.Framework.SdlWindowing;

public class SdlHost : Host {
	ConcurrentQueue<Action> scheduledActions = new();
	internal void Shedule ( Action action ) {
		scheduledActions.Enqueue( action );
	}
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

			if ( SDL.SDL_PollEvent( out e ) == 0 ) {
				Thread.Sleep( 1 );
				continue;
			}

			if ( e.type == SDL.SDL_EventType.SDL_WINDOWEVENT ) {
				var @event = e.window;
				if ( windowsById.TryGetValue( @event.windowID, out var window ) ) {
					window.OnEvent( @event );
				}
			}
			else if ( e.type == SDL.SDL_EventType.SDL_MOUSEMOTION ) {
				var @event = e.motion;
				if ( windowsById.TryGetValue( @event.windowID, out var window ) ) {
					window.OnEvent( @event );
				}
			}
			else if ( e.type == SDL.SDL_EventType.SDL_MOUSEWHEEL ) {
				var @event = e.wheel;
				if ( windowsById.TryGetValue( @event.windowID, out var window ) ) {
					window.OnEvent( @event );
				}
			}
			else if ( e.type is SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN or SDL.SDL_EventType.SDL_MOUSEBUTTONUP ) {
				var @event = e.button;
				if ( windowsById.TryGetValue( @event.windowID, out var window ) ) {
					window.OnEvent( @event );
				}
			}
			else if ( e.type is SDL.SDL_EventType.SDL_KEYDOWN or SDL.SDL_EventType.SDL_KEYUP ) {
				var @event = e.key;
				if ( windowsById.TryGetValue( @event.windowID, out var window ) ) {
					window.OnEvent( @event );
				}
			}
		}

		isRunning = false;
	}

	Dictionary<uint, SdlWindow> windowsById = new();
	public override Window CreateWindow ( RenderingApi renderingApi ) {
		var window = new SdlWindow( this, renderingApi );
		scheduledActions.Enqueue( () => {
			window.Init();
			windowsById[window.Id] = window;
		} );
		return window;
	}

	public override IEnumerable<RenderingApi> SupportedRenderingApis {
		get {
			yield return RenderingApi.OpenGl;
		}
	}

	internal void destroyWindow ( SdlWindow window ) {
		windowsById.Remove( window.Id );
		SDL.SDL_DestroyWindow( window.Pointer );
		window.Pointer = 0;
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
