using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;
using PixelType = OpenTK.Graphics.OpenGL4.PixelType;

namespace Vit.Framework.Graphics.OpenGl.Textures;

public class Texture2DStorage : DisposableObject, ITexture2D {
	public readonly int Handle;
	public Size2<uint> Size { get; }
	public Graphics.Rendering.Textures.PixelFormat Format { get; }

	public Texture2DStorage ( Size2<uint> size, Graphics.Rendering.Textures.PixelFormat format ) {
		var internalFormat = OpenGlApi.internalFormats[format];
		Size = size;
		Format = format;

		Handle = GL.GenTexture();
		GL.BindTexture( TextureTarget.Texture2D, Handle );
		//var mips = uint.Log2( uint.Max( size.Width, size.Height ) ) + 1;
		GL.TextureStorage2D( Handle, 1, internalFormat, (int)size.Width, (int)size.Height );
	}

	public unsafe void Upload<TPixel> ( ReadOnlySpan<TPixel> data ) where TPixel : unmanaged {
		Debug.Assert( Format == Graphics.Rendering.Textures.PixelFormat.Rgba8 );
		GL.TextureSubImage2D( Handle, 0, 0, 0, (int)Size.Width, (int)Size.Height, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, (nint)data.Data() );

		GL.BindTexture( TextureTarget.Texture2D, Handle );
		//GL.GenerateMipmap( GenerateMipmapTarget.Texture2D );
	}

	public ITexture2DView CreateView () {
		return new Texture2DView( this );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteTexture( Handle );
	}
}
