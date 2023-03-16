using SDL2;
using Vit.Framework.Math;
using Vit.Framework.Windowing;

namespace Vit.Framework.SdlWindowing;

class SdlWindow : Window {
	string title = "New Window";
	public override string Title {
		get => title;
		set {
			title = value;
			if ( Pointer == 0 )
				return;

			host.Shedule( () => SDL.SDL_SetWindowTitle( Pointer, title ) );
		}
	}

	Size2<int> size = new( 640, 480 );
	public override Size2<int> Size {
		get => size;
		set {
			size = value;
			if ( Pointer == 0 )
				return;

			host.Shedule( () => SDL.SDL_SetWindowSize( Pointer, Width, Height ) );
		}
	}

	SdlHost host;
	public nint Pointer;
	public uint Id;
	public SdlWindow ( SdlHost host ) {
		this.host = host;
	}

	public void Init () {
		Pointer = SDL.SDL_CreateWindow( title, SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, Width, Height, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN );
		if ( Pointer == IntPtr.Zero ) {
			SdlHost.ThrowSdl( "window creation failed" );
		}

		Id = SDL.SDL_GetWindowID( Pointer );
	}

	public void OnEvent ( SDL.SDL_WindowEvent e ) {
		if ( e.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE )
			Quit();
	}

	public void OnEvent ( SDL.SDL_MouseMotionEvent e ) {

	}

	public void OnEvent ( SDL.SDL_MouseButtonEvent e ) {

	}

	public void OnEvent ( SDL.SDL_MouseWheelEvent e ) {

	}

	public void OnEvent ( SDL.SDL_KeyboardEvent e ) {
		
	}

	protected override void Dispose ( bool disposing ) {
		host.Shedule( () => {
			host.destroyWindow( this );
		} );
	}
}
