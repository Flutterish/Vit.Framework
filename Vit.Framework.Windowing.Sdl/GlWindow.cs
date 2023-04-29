using SDL2;
using Vit.Framework.Graphics.OpenGl;
using Vit.Framework.Graphics.OpenGl.Queues;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;

namespace Vit.Framework.Windowing.Sdl;

class GlWindow : SdlWindow, IGlWindow {
	public GlWindow ( SdlHost host ) : base( host, GraphicsApiType.OpenGl ) { 
		
	}

	public override (ISwapchain swapchain, IRenderer renderer) CreateSwapchain ( GraphicsApi api, SwapChainArgs args ) {
		// TODO the args mean we might need to recreate the window
		if ( api is not OpenGlApi gl )
			throw new ArgumentException( "Graphics API must be an OpenGl API created from the same host as this window", nameof( api ) );

		var swapchain = new GlSwapchain( this );
		var renderer = new GlRenderer( gl );

		return (swapchain, renderer);
	}

	public nint CreateContext () {
		var context = SDL.SDL_GL_CreateContext( Pointer );
		if ( context == 0 )
			SdlHost.ThrowSdl( "gl context creation" );

		return context;
	}

	public void MakeCurrent ( nint context ) {
		SDL.SDL_GL_MakeCurrent( Pointer, context );
	}

	public void SwapBackbuffer () {
		SDL.SDL_GL_SwapWindow( Pointer );
	}
}
