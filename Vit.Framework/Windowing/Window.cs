using Vit.Framework.Math;

namespace Vit.Framework.Windowing;

public abstract class Window {
	public abstract int Width { get; set; }
	public abstract int Height { get; set; }

	public Size2<int> Size {
		get => new( Width, Height );
		set {
			Width = value.Width;
			Height = value.Height;
		}
	}
}
