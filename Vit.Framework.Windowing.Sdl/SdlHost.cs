using SDL2;
using System.Collections.Concurrent;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Platform;
using Vit.Framework.Threading;

namespace Vit.Framework.Windowing.Sdl;

public class SdlHost : Host {
	ConcurrentQueue<Action> scheduledActions = new();
	internal void Shedule ( Action action ) {
		scheduledActions.Enqueue( action );
	}

	SdlEventThread eventThread;
	public SdlHost ( App primaryApp ) : base( primaryApp ) {
		eventThread = new( this );
		PrimaryApp.ThreadRunner.RegisterThread( eventThread );
	}

	class SdlEventThread : AppThread {
		SdlHost host;
		ConcurrentQueue<Action> scheduledActions => host.scheduledActions;
		Dictionary<uint, SdlWindow> windowsById => host.windowsById;
		public SdlEventThread ( SdlHost host ) : base( "SDL Event Thread" ) {
			this.host = host;
		}

		protected override void Initialize () {
			if ( SDL.SDL_Init( SDL.SDL_INIT_VIDEO ) < 0 ) {
				ThrowSdl( "sdl initialisation failed" );
			}
		}

		protected override void Loop () {
			SDL.SDL_Event e;

			while ( scheduledActions.TryDequeue( out var action ) )
				action();

			while ( SDL.SDL_PollEvent( out e ) != 0 ) {
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

			Sleep( 1 );
		}
	}

	Dictionary<uint, SdlWindow> windowsById = new();
	public override Window CreateWindow ( RenderingApi renderingApi ) {
		if ( IsDisposed )
			throw new InvalidOperationException( "Cannot create new windows with a disposed host" );

		var window = new SdlWindow( this, renderingApi );
		scheduledActions.Enqueue( () => {
			window.Init();
			windowsById[window.Id] = window;
		} );
		return window;
	}

	public override IEnumerable<RenderingApi> SupportedRenderingApis { get; } = new[] { 
		RenderingApi.OpenGl,
		RenderingApi.Vulkan
	};

	internal void destroyWindow ( SdlWindow window ) {
		windowsById.Remove( window.Id );
		SDL.SDL_DestroyWindow( window.Pointer );
		window.Pointer = 0;
	}

	public override void Dispose ( bool isDisposing ) {
		foreach ( var i in windowsById )
			i.Value.Dispose();

		Task.Run( async () => {
			while ( windowsById.Any() )
				await Task.Delay( 1 );

			await eventThread.StopAsync();
			SDL.SDL_Quit();
		} );

		GC.SuppressFinalize( this );
	}

	~SdlHost () {
		Dispose();
	}

	internal static void ThrowSdl ( string reason ) { // TODO handle this better than with exceptions
		throw new InvalidOperationException( $"SDL Error ({reason}): {SDL.SDL_GetError()}" );
	}
}
