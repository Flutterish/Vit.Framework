using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.Rendering.Textures;

public interface ITexture : IDisposable {
	Size2<uint> Size { get; }
	PixelFormat Format { get; }
}
