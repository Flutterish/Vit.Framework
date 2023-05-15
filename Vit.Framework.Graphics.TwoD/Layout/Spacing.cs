using System.Numerics;

namespace Vit.Framework.Graphics.TwoD.Layout;

public struct Spacing<T> where T : INumber<T> {
	public T Bottom;
	public T Top;
	public T Left;
	public T Right;

	public T Vertical {
		get => Top + Bottom;
		set => Top = Bottom = value;
	}
	public T Horizontal {
		get => Left + Right;
		set => Left = Right = value;
	}

	public Spacing ( T all ) {
		Top = Bottom = Left = Right = all;
	}
}
