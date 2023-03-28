using SDL2;
using Vit.Framework.Graphics.Rendering;
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

			host.Shedule( () => SDL.SDL_SetWindowSize( Pointer, Width, Height ) );
		}
	}

	SdlHost host;
	public nint Pointer;
	public uint Id;
	RenderingApi renderingApi;
	public SdlWindow ( SdlHost host, RenderingApi renderingApi ) : base( renderingApi ) {
		this.renderingApi = renderingApi;
		this.host = host;
	}

	public void Init () {
		var windowFlags = SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN;
		if ( renderingApi == RenderingApi.OpenGl ) {
			SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 3 );
			SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, 1 );
			SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE );

			windowFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL;
		}
		else if ( renderingApi == RenderingApi.Vulkan ) {
			windowFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN;
		}
		else if ( renderingApi == RenderingApi.Direct3D11 ) {
			
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