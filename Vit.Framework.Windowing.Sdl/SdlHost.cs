using SDL2;
using System.Collections.Concurrent;
using System.Diagnostics;
using Vit.Framework.Graphics.Direct3D11;
using Vit.Framework.Graphics.OpenGl;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Specialisation;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Input;
using Vit.Framework.Interop;
using Vit.Framework.Platform;
using Vit.Framework.Threading;
using Vit.Framework.Windowing.Sdl.Input;

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

		protected override bool Initialize () {
			if ( SDL.SDL_Init( SDL.SDL_INIT_VIDEO ) < 0 ) {
				ThrowSdl( "sdl initialisation failed" );
			}

			SDL.SDL_StartTextInput();
			SDL.SDL_AddEventWatch( eventFilter = new SDL.SDL_EventFilter( eventWatch ), 0 );
			return true;
		}

		protected override void Loop () {
			while ( scheduledActions.TryDequeue( out var action ) )
				action();

			SDL.SDL_PumpEvents();
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
			else if ( e.type is SDL.SDL_EventType.SDL_TEXTINPUT ) {
				var @event = e.text;
				if ( windowsById.TryGetValue( @event.windowID, out var window ) ) {
					window.OnEvent( @event );
				}
			}
		}
	}

	Dictionary<uint, SdlWindow> windowsById = new();
	public override Task<Window> CreateWindow () {
		if ( IsDisposed )
			throw new InvalidOperationException( "Cannot create new windows with a disposed host" );

		SdlWindow window = new( this );

		window.SdlWindowCreated += window => {
			windowsById[window.Id] = window;
		};

		TaskCompletionSource<Window> taskSource = new();
		scheduledActions.Enqueue( () => {
			window.Init();
			taskSource.SetResult( window );
		} );
		return taskSource.Task;
	}

	internal void destroyWindow ( SdlWindow window ) {
		SDL.SDL_DestroyWindow( window.Pointer );
		windowsById.Remove( window.Id );
		window.Pointer = 0;
	}

	public override GraphicsApi CreateGraphicsApi ( GraphicsApiType api, IEnumerable<RenderingCapabilities> capabilities ) => api switch {
		var x when x == OpenGlApi.GraphicsApiType => new OpenGlApi( capabilities ),
		var x when x == VulkanApi.GraphicsApiType => createVulkanApi( capabilities ),
		var x when x == Direct3D11Api.GraphicsApiType => new Direct3D11Api( capabilities ),
		_ => throw new ArgumentException( $"Unsupported rendering api: {api}", nameof(api) )
	};

	static readonly CString VK_LAYER_KHRONOS_validation = CString.CreateStaticPinned( "VK_LAYER_KHRONOS_validation" );
	static readonly CString VK_EXT_debug_utils = CString.CreateStaticPinned( "VK_EXT_debug_utils" );

	unsafe VulkanApi createVulkanApi ( IEnumerable<RenderingCapabilities> capabilities ) {
		List<CString> layers = new();
		List<CString> extensions = new();

		if ( Debugger.IsAttached ) {
			layers.Add( VK_LAYER_KHRONOS_validation );
			extensions.Add( VK_EXT_debug_utils );
		}

		foreach ( var i in capabilities ) {
			switch ( i ) {
				case RenderingCapabilities.RenderOffscreen:
					break;

				case RenderingCapabilities.DrawToWindow:
					var windowFlags = SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN; // HACK we create a vulkan window to fetch the required extensions
					var windowPointer = SDL.SDL_CreateWindow( "", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, 0, 0, windowFlags );

					SDL.SDL_Vulkan_GetInstanceExtensions( windowPointer, out var count, null );
					nint[] pointers = new nint[count];
					SDL.SDL_Vulkan_GetInstanceExtensions( windowPointer, out count, pointers );
					extensions.AddRange( pointers.Select( x => new CString( x ) ) );

					SDL.SDL_DestroyWindow( windowPointer );
					break;

				default:
					throw new ArgumentException( $"Capability not supported: {i}", nameof(capabilities) );
			}
		}

		return new VulkanApi( new VulkanInstance( extensions, layers ), capabilities );
	}

	public override IEnumerable<GraphicsApiType> SupportedRenderingApis { get; } = new[] { 
		OpenGlApi.GraphicsApiType,
		VulkanApi.GraphicsApiType,
		Direct3D11Api.GraphicsApiType
	};


	SdlClipboard clipboard = new();
	public override Clipboard GetClipboard () {
		return clipboard;
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
