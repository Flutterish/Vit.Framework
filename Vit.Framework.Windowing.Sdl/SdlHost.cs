using SDL2;
using System.Collections.Concurrent;
using System.Diagnostics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Interop;
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

			SDL.SDL_AddEventWatch( eventFilter = new SDL.SDL_EventFilter( eventWatch ), 0 );
		}

		protected override void Loop () {
			while ( scheduledActions.TryDequeue( out var action ) )
				action();

			SDL.SDL_PumpEvents();
			Sleep( 1 );
		}

		SDL.SDL_EventFilter? eventFilter;
		unsafe int eventWatch ( nint _, nint @event ) {
			var ep = (SDL.SDL_Event*)@event;
			CheckEvent( *ep );
			return 1;
		}

		void CheckEvent ( SDL.SDL_Event e ) {
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
	}

	Dictionary<uint, SdlWindow> windowsById = new();
	public override Window CreateWindow ( GraphicsApiType renderingApi ) {
		if ( IsDisposed )
			throw new InvalidOperationException( "Cannot create new windows with a disposed host" );

		SdlWindow window = renderingApi switch {
			GraphicsApiType.Vulkan => new VulkanWindow( this ),
			//RenderingApi.Direct3D11 => new Direct3D11Window( this ),
			//RenderingApi.OpenGl => new GlWindow( this ),
			_ => throw new ArgumentException( $"Unsupported rendering api: {renderingApi}", nameof(renderingApi) )
		};
		scheduledActions.Enqueue( () => {
			window.Init();
			windowsById[window.Id] = window;
		} );
		return window;
	}

	public override GraphicsApi CreateGraphicsApi ( GraphicsApiType api, IEnumerable<RenderingCapabilities> capabilities ) => api switch {
		//RenderingApi.OpenGl => new OpenGlRenderer( capabilities ),
		GraphicsApiType.Vulkan => createVulkanApi( capabilities ),
		//RenderingApi.Direct3D11 => new Direct3D11Renderer( capabilities ),
		_ => throw new ArgumentException( $"Unsupported rendering api: {api}", nameof(api) )
	};

	VulkanApi createVulkanApi ( IEnumerable<RenderingCapabilities> capabilities ) {
		List<CString> layers = new();
		List<CString> extensions = new();

		if ( Debugger.IsAttached ) {
			layers.Add( "VK_LAYER_KHRONOS_validation" );
			extensions.Add( "VK_EXT_debug_utils" );
		}

		foreach ( var i in capabilities ) {
			switch ( i ) {
				case RenderingCapabilities.RenderOffscreen:
					break;

				case RenderingCapabilities.DrawToWindow:
					if ( windowsById.Values.FirstOrDefault() is not SdlWindow window )
						throw new Exception( $"In order to enabe {i}, some window must be created (SLD 3.0 limitation, to be removed in SDL 4.0)" );
					SDL.SDL_Vulkan_GetInstanceExtensions( window.Pointer, out var count, null );
					nint[] pointers = new nint[count];
					SDL.SDL_Vulkan_GetInstanceExtensions( window.Pointer, out count, pointers );
					extensions.AddRange( pointers.Select( x => new CString( x ) ) );
					break;

				default:
					throw new ArgumentException( $"Capability not supported: {i}", nameof(capabilities) );
			}
		}

		return new VulkanApi( new VulkanInstance( extensions, layers ), capabilities );
	}

	public override IEnumerable<GraphicsApiType> SupportedRenderingApis { get; } = new[] { 
		//GraphicsApiType.OpenGl,
		GraphicsApiType.Vulkan,
		//GraphicsApiType.Direct3D11
	};

	internal void destroyWindow ( SdlWindow window ) {
		SDL.SDL_DestroyWindow( window.Pointer );
		windowsById.Remove( window.Id );
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
