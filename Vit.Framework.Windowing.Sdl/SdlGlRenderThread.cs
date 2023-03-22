using OpenTK.Graphics.ES30;
using SDL2;
using Vit.Framework.Graphics.OpenGl;
using Vit.Framework.Threading;

namespace Vit.Framework.Windowing.Sdl;

class SdlGlRenderThread : AppThread {
	SdlWindow window;
	public SdlGlRenderThread ( SdlWindow window, string name ) : base( name ) {
		this.window = window;
	}

	nint glContext;
	static bool bindingsInitialized;
	static object loadLock = new();
	protected override void Initialize () {
		glContext = SDL.SDL_GL_CreateContext( window.Pointer );
		if ( glContext == 0 )
			SdlHost.ThrowSdl( "gl context creation" );

		lock ( loadLock ) {
			if ( !bindingsInitialized ) {
				InitializeGlBindings();
				bindingsInitialized = true;
			}
		}

		var rng = new Random();
		GL.ClearColor( rng.NextSingle(), rng.NextSingle(), rng.NextSingle(), 1 );

		vao = GL.GenVertexArray();
		GL.BindVertexArray( vao );

		vbo = GL.GenBuffer();
		GL.BindBuffer( BufferTarget.ArrayBuffer, vbo );
		GL.BufferData( BufferTarget.ArrayBuffer, 6 * sizeof(float), new float[] {
			0, 0.5f,
			0.5f, -0.5f,
			-0.5f, -0.5f
		}, BufferUsageHint.StaticDraw );

		shader = GL.CreateProgram();
		var vs = GL.CreateShader( ShaderType.VertexShader );
		GL.ShaderSource( vs, @"#version 330 core
			layout (location = 0) in vec2 pos;
			out vec3 fc;

			vec3 colors[3] = vec3[](
				vec3(1.0, 0.0, 0.0),
				vec3(0.0, 1.0, 0.0),
				vec3(0.0, 0.0, 1.0)
			);

			void main () {
				gl_Position = vec4(pos, 0, 1);
				fc = colors[gl_VertexID];
			}
		" );
		var fs = GL.CreateShader( ShaderType.FragmentShader );
		GL.ShaderSource( fs, @"#version 330 core
			out vec4 FragColor;
			in vec3 fc;

			void main()
			{
				FragColor = vec4(fc, 1.0f);
			}
		" );

		GL.CompileShader( vs );
		GL.CompileShader( fs );

		GL.AttachShader( shader, vs );
		GL.AttachShader( shader, fs );
		GL.LinkProgram( shader );

		GL.DetachShader( shader, vs );
		GL.DetachShader( shader, fs );
		GL.DeleteShader( fs );
		GL.DeleteShader( vs );

		GL.VertexAttribPointer( 0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0 );
		GL.EnableVertexAttribArray( 0 );
	}

	int vbo;
	int vao;
	int shader;
	protected override void Loop () {
		SDL.SDL_GL_MakeCurrent( window.Pointer, glContext );
		GL.BindVertexArray( vao );

		GL.Clear( ClearBufferMask.ColorBufferBit );

		GL.UseProgram( shader );
		GL.DrawArrays( PrimitiveType.Triangles, 0, 3 );

		SDL.SDL_GL_SwapWindow( window.Pointer );
		GL.Finish();

		Sleep( 1 );
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
