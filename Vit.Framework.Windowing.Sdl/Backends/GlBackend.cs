using SDL2;
using Vit.Framework.Graphics.OpenGl;
using Vit.Framework.Graphics.OpenGl.Windowing;
using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Windowing.Sdl.Backends;

class GlBackend : SdlBackend {
	public override void InitializeHints ( WindowSurfaceArgs args, ref SDL.SDL_WindowFlags flags ) {
		var multisamples = (int)args.Multisample.Ideal;
		var minDepth = (int)args.Depth.Minimum;
		var minStencil = (int)args.Stencil.Minimum;

		flags |= SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL;
		SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 4 );
		SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, 6 );
		SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE );

		SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, minDepth );
		SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, minStencil );
		SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_DOUBLEBUFFER, 1 );
		if ( multisamples > 1 ) {
			SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_MULTISAMPLEBUFFERS, 1 );
			SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_MULTISAMPLESAMPLES, multisamples );
		}
		else {
			SDL.SDL_GL_SetAttribute( SDL.SDL_GLattr.SDL_GL_MULTISAMPLEBUFFERS, 0 );
		}
	}

	public override WindowGraphicsSurface CreateSurface ( GraphicsApi api, WindowSurfaceArgs args, SdlWindow window ) {
		if ( api is not OpenGlApi gl )
			throw new ArgumentException( "Graphics API must be an OpenGl API created from the same host as this window", nameof( api ) );

		return new GlWindowSurface( gl, args, window );
	}
}
