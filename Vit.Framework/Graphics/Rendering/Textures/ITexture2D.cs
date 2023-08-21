using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.Rendering.Textures;

public interface ITexture2D : IDisposable { // TODO this is more of a data store that we can decouple from the view (and sampler)
	Size2<uint> Size { get; }
	PixelFormat Format { get; }
}
