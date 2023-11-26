using System.Numerics;

namespace Vit.Framework.TwoD.Layout;

public struct RelativeSpacing<T> where T : INumber<T> {
	public LayoutUnit<T> Bottom;
	public LayoutUnit<T> Top;
	public LayoutUnit<T> Left;
	public LayoutUnit<T> Right;

	public LayoutUnit<T> Vertical {
		get => Top + Bottom;
		set => Top = Bottom = value;
	}
	public LayoutUnit<T> Horizontal {
		get => Left + Right;
		set => Left = Right = value;
	}

	public RelativeSpacing ( LayoutUnit<T> all ) {
		Top = Bottom = Left = Right = all;
	}
}
