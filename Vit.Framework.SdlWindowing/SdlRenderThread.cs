using OpenTK.Graphics.ES30;
using SDL2;
using Vit.Framework.OpenGLRenderer;
using Vit.Framework.Threading;

namespace Vit.Framework.SdlWindowing;

class SdlGlRenderThread : AppThread {
	SdlWindow window;
	public SdlGlRenderThread ( SdlWindow window, string name ) : base( name ) {
		this.window = window;
	}

	nint glContext;
	static bool bindingsInitialized;
	object loadLock = new();
	protected override void Initialize () {
		lock ( loadLock ) {
			if ( !bindingsInitialized ) {
				InitializeGlBindings();
				bindingsInitialized = true;
			}
		}

		glContext = SDL.SDL_GL_CreateContext( window.Pointer );
		if ( glContext == 0 )
			SdlHost.ThrowSdl( "gl context creation" );

		var rng = new Random();
		GL.ClearColor( rng.NextSingle(), rng.NextSingle(), rng.NextSingle(), 1 );
	}

	protected override void Loop () {
		SDL.SDL_GL_MakeCurrent( window.Pointer, glContext );

		GL.Clear( ClearBufferMask.ColorBufferBit );

		SDL.SDL_GL_SwapWindow( window.Pointer );
		GL.Finish();

		Thread.Sleep( 1 );
	}

	private static void InitializeGlBindings () {
		var ctx = new WglBindingsContext();

		OpenTK.Graphics.ES11.GL.LoadBindings( ctx );
		OpenTK.Graphics.ES20.GL.LoadBindings( ctx );
		OpenTK.Graphics.ES30.GL.LoadBindings( ctx );
		OpenTK.Graphics.OpenGL.GL.LoadBindings( ctx );
		OpenTK.Graphics.OpenGL4.GL.LoadBindings( ctx );
	}
}
