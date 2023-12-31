﻿using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Textures;

public class Texture2DView : DisposableObject, ITexture2DView {
	public readonly int Handle;
	public IDeviceTexture2D Source { get; }
	public Texture2DView ( Texture2DStorage source ) {
		var format = OpenGlApi.internalFormats[source.Format];
		Source = source;
		Handle = GL.GenTexture();

		GL.TextureView( Handle, TextureTarget.Texture2D, source.Handle, (PixelInternalFormat)format, 0, 1, 0, 1 );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteTexture( Handle );
	}
}
