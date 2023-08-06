using Vit.Framework.Mathematics;

namespace Vit.Framework.Windowing;

public interface IWindow {
	Size2<uint> PixelSize { get; }
	public uint PixelWidth => PixelSize.Width;
	public uint PixelHeight => PixelSize.Height;

	bool IsClosed { get; }
}
