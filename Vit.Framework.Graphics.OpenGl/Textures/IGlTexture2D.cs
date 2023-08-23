using Vit.Framework.Graphics.Rendering.Textures;

namespace Vit.Framework.Graphics.OpenGl.Textures;

public interface IGlTexture2D : ITexture2D {
	GlTextureType Type { get; }
}

public enum GlTextureType {
	Storage,
	PixelBuffer
}