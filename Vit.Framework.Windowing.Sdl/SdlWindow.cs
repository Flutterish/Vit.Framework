using SDL2;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Input;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Windowing.Sdl;

abstract class SdlWindow : Window {
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

			host.Shedule( () => {
				SDL.SDL_SetWindowSize( Pointer, Width, Height );
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

	public void Init () {
		var windowFlags = SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
		if ( renderingApi == GraphicsApiType.OpenGl ) {
			SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 4 );
			SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, 6 );
			SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE );

			windowFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL;
		}
		else if ( renderingApi == GraphicsApiType.Vulkan ) {
			windowFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN;
		}
		else if ( renderingApi == GraphicsApiType.Direct3D11 ) {
			
		}

		Pointer = SDL.SDL_CreateWindow( title, SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, Width, Height, windowFlags );
		if ( Pointer == 0 ) {
			SdlHost.ThrowSdl( "window creation failed" );
		}

		Id = SDL.SDL_GetWindowID( Pointer );
		OnInitialized();
	}

	public void OnEvent ( SDL.SDL_WindowEvent e ) {
		if ( e.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE )
			Quit();
		else if ( e.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED ) {
			SDL.SDL_GetWindowSize( Pointer, out size.Width, out size.Height );
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
		if ( size != new Size2<int>( width, height ) ) {
			size = new Size2<int>( width, height );
			OnResized();
		}
	}

	protected override void Dispose ( bool disposing ) {
		host.Shedule( () => {
			host.destroyWindow( this );
		} );
	}
}