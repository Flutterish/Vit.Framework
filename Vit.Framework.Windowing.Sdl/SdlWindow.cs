using SDL2;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Input;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Windowing.Sdl;

public abstract class SdlWindow : Window {
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

	Size2<uint> size = new( 640, 480 );
	public override Size2<uint> Size {
		get => size;
		set {
			size = value;
			if ( Pointer == 0 )
				return;

			host.Shedule( () => {
				SDL.SDL_SetWindowSize( Pointer, (int)Width, (int)Height );
				OnResized();
			} );
		}
	}

	SdlHost host;
	public nint Pointer;
	public uint Id;
	GraphicsApiType renderingApi;
	public SdlWindow ( SdlHost host, GraphicsApiType renderingApi ) : base( renderingApi ) {
		this.renderingApi = renderingApi;
		this.host = host;
	}

	protected abstract void InitializeHints ( ref SDL.SDL_WindowFlags flags );
	public void Init () {
		create();
		OnInitialized();
	}

	void create () {
		var windowFlags = SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
		InitializeHints( ref windowFlags );

		Pointer = SDL.SDL_CreateWindow( title, SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, (int)Width, (int)Height, windowFlags );
		if ( Pointer == 0 ) {
			SdlHost.ThrowSdl( "window creation failed" );
		}

		Id = SDL.SDL_GetWindowID( Pointer );
		SdlWindowCreated?.Invoke( this );
	}

	protected Task Recreate () {
		var source = new TaskCompletionSource();
		host.Shedule( () => {
			host.destroyWindow( this );
			create();
			source.SetResult();
		} );
		return source.Task;
	}

	internal event Action<SdlWindow>? SdlWindowCreated;

	public void OnEvent ( SDL.SDL_WindowEvent e ) {
		if ( e.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE )
			Quit();
		else if ( e.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED ) {
			SDL.SDL_GetWindowSize( Pointer, out var width, out var height );
			size.Width = (uint)width;
			size.Height = (uint)height;
			OnResized();
		}
	}

	public void OnEvent ( SDL.SDL_MouseMotionEvent e ) {
		OnCursorMoved( new( e.x, e.y ) );
	}

	public void OnEvent ( SDL.SDL_MouseButtonEvent e ) {

	}

	public void OnEvent ( SDL.SDL_MouseWheelEvent e ) {

	}

	public void OnEvent ( SDL.SDL_KeyboardEvent e ) {
		if ( KeyExtensions.GetKeyByScanCode( (int)e.keysym.scancode ) is Key key ) {
			if ( e.state == 0 )
				OnPhysicalKeyUp( key );
			else
				OnPhysicalKeyDown( key );
		}
	}

	public void CheckResize () {
		SDL.SDL_GetWindowSize( Pointer, out var width, out var height );
		var newSize = new Size2<uint>( (uint)width, (uint)height );
		if ( size != newSize ) {
			size = newSize;
			OnResized();
		}
	}

	protected override void Dispose ( bool disposing ) {
		host.Shedule( () => {
			host.destroyWindow( this );
		} );
	}
}