using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Textures;

public class Texture2DView : DisposableObject, ITexture2DView {
	public readonly int Handle;
	public ITexture2D Source { get; }
	public Texture2DView ( Texture2DStorage source ) {
		Source = source;
		Handle = GL.GenTexture();

		GL.TextureView( Handle, TextureTarget.Texture2D, source.Handle, PixelInternalFormat.Rgba8, 0, 1, 0, 1 );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteTexture( Handle );
	}
}
