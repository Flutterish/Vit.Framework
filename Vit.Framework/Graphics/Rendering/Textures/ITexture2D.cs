using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.Rendering.Textures;

public interface ITexture2D : IDisposable { // TODO resize/allocate methods
	Size2<uint> Size { get; }
	PixelFormat Format { get; }

	ITexture2DView CreateView ();
}
