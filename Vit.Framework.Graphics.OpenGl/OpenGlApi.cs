using OpenTK;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Specialisation;

namespace Vit.Framework.Graphics.OpenGl;

public class OpenGlApi : GraphicsApi {
	public static readonly GraphicsApiType GraphicsApiType = new() {
		KnownName = KnownGraphicsApiName.OpenGl,
		Name = "OpenGL Core",
		Version = 460
	};

	static IBindingsContext? bindings;
	static object loadLock = new();
	public OpenGlApi ( IEnumerable<RenderingCapabilities> capabilities ) : base( GraphicsApiType, capabilities ) {
		
	}

	internal static void loadBindings () {
		lock ( loadLock ) {
			if ( bindings == null ) {
				InitializeGlBindings();
			}
		}
	}

	private static void InitializeGlBindings () {
		bindings = new WglBindingsContext();

		OpenTK.Graphics.ES11.GL.LoadBindings( bindings );
		OpenTK.Graphics.ES20.GL.LoadBindings( bindings );
		OpenTK.Graphics.ES30.GL.LoadBindings( bindings );
		OpenTK.Graphics.OpenGL.GL.LoadBindings( bindings );
		OpenTK.Graphics.OpenGL4.GL.LoadBindings( bindings );
	}

	protected override void Dispose ( bool disposing ) {
		
	}

	public static Dictionary<Graphics.Rendering.Textures.PixelFormat, SizedInternalFormat> internalFormats = new() {
		[Graphics.Rendering.Textures.PixelFormat.Rgba8] = SizedInternalFormat.Rgba8,
		[Graphics.Rendering.Textures.PixelFormat.D24] = SizedInternalFormat.DepthComponent24,
		[Graphics.Rendering.Textures.PixelFormat.D32f] = SizedInternalFormat.DepthComponent32f,
		[Graphics.Rendering.Textures.PixelFormat.D24S8ui] = SizedInternalFormat.Depth24Stencil8,
		[Graphics.Rendering.Textures.PixelFormat.D32fS8ui] = SizedInternalFormat.Depth32fStencil8,
		[Graphics.Rendering.Textures.PixelFormat.S8ui] = SizedInternalFormat.StencilIndex8
	};
}
