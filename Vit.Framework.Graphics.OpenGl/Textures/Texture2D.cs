using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Textures;

public class Texture2D : DisposableObject, ITexture {
	public readonly int Handle;
	public Size2<uint> Size { get; }
	public Graphics.Rendering.Textures.PixelFormat Format { get; }
	public Texture2D ( Size2<uint> size, Graphics.Rendering.Textures.PixelFormat format ) {
		Debug.Assert( format.IsRGBA32 );
		Size = size;
		Format = format;

		GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder );
		GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder );
		GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, new float[] { 1, 1, 1, 1 } );
		GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest );
		GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest );
		Handle = GL.GenTexture();
		GL.BindTexture( TextureTarget.Texture2D, Handle );
		GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)size.Width, (int)size.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, 0 );
	}

	public unsafe void Upload<TPixel> ( ReadOnlySpan<TPixel> data ) where TPixel : unmanaged {
		GL.BindTexture( TextureTarget.Texture2D, Handle );
		GL.TexSubImage2D( TextureTarget.Texture2D, 0, 0, 0, (int)Size.Width, (int)Size.Height, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, (nint)data.Data() );
		GL.GenerateMipmap( GenerateMipmapTarget.Texture2D );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteTexture( Handle );
	}
}
