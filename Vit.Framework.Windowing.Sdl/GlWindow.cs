using SDL2;
using Vit.Framework.Graphics.OpenGl;
using Vit.Framework.Graphics.OpenGl.Windowing;
using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Windowing.Sdl;

class GlWindow : SdlWindow, IGlWindow {
	public GlWindow ( SdlHost host ) : base( host, OpenGlApi.GraphicsApiType ) { 
		
	}

	int multisamples = 1;
	int minDepth = 0;
	int minStencil = 0;
	protected override void InitializeHints ( ref SDL.SDL_WindowFlags flags ) {
		SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 4 );
		SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, 6 );
		SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE );
		SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_MULTISAMPLESAMPLES, multisamples );
		SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, minDepth );
		SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, minStencil );

		flags |= SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL;
	}

	bool swapchainCreated;
	public override WindowGraphicsSurface CreateGraphicsSurface ( GraphicsApi api, WindowSurfaceArgs args ) {
		if ( swapchainCreated )
			throw new NotImplementedException( "Surface recreation not implemented" );
		swapchainCreated = true;
		
		if ( api is not OpenGlApi gl )
			throw new ArgumentException( "Graphics API must be an OpenGl API created from the same host as this window", nameof( api ) );

		multisamples = (int)args.Multisample.Ideal;
		minDepth = (int)args.Depth.Minimum;
		minStencil = (int)args.Stencil.Minimum;
		Recreate().Wait(); // TODO this stalls on singlethreaded, probably make the swapchain (and window) creation a task

		return new GlWindowSurface( gl, args, this );
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
