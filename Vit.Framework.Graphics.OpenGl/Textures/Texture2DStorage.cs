using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Textures;

public class Texture2DStorage : DisposableObject, IDeviceTexture2D, IGlTexture2D {
	public GlTextureType Type => GlTextureType.Storage;
	public readonly int Handle;
	public Size2<uint> Size { get; }
	public Graphics.Rendering.Textures.PixelFormat Format { get; }

	public Texture2DStorage ( Size2<uint> size, Graphics.Rendering.Textures.PixelFormat format ) {
		var internalFormat = OpenGlApi.internalFormats[format];
		Size = size;
		Format = format;

		Handle = GL.GenTexture();
		GL.BindTexture( TextureTarget.Texture2D, Handle );
		GL.TextureStorage2D( Handle, 1, internalFormat, (int)size.Width, (int)size.Height );
	}

	public ITexture2DView CreateView () {
		return new Texture2DView( this );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteTexture( Handle );
	}
}
