using System.Numerics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.Layout;

public struct RelativePoint2<T> where T : INumber<T> {
	public LayoutUnit<T> X;
	public LayoutUnit<T> Y;

	public RelativePoint2 ( LayoutUnit<T> x, LayoutUnit<T> y ) {
		X = x;
		Y = y;
	}

	public Point2<T> GetValue ( Size2<T> availableSpace ) => new() {
		X = X.GetValue( availableSpace.Width ),
		Y = Y.GetValue( availableSpace.Height )
	};

	public static implicit operator RelativePoint2<T> ( Point2<T> size ) => new() {
		X = size.X,
		Y = size.Y
	};

	public static implicit operator RelativePoint2<T> ( Anchor anchor ) => new() {
		X = anchor.HasFlag( Anchor.HorizontalCentre ) ? (T.One / (T.One + T.One)).Relative() : anchor.HasFlag( Anchor.Right ) ? T.One.Relative() : T.Zero,
		Y = anchor.HasFlag( Anchor.VerticalCentre ) ? (T.One / (T.One + T.One)).Relative() : anchor.HasFlag( Anchor.Top ) ? T.One.Relative() : T.Zero
	};

	public override string ToString () {
		return $"({X}, {Y})";
	}
}

[Flags]
public enum Anchor {
	Invalid = 0,

	Bottom = 0b01,
	VerticalCentre = 0b11,
	Top = 0b10,

	Left = 0b0100,
	HorizontalCentre = 0b1100,
	Right = 0b1000,

	BottomLeft = Bottom | Left,
	BottomCentre = Bottom | HorizontalCentre,
	BottomRight = Bottom | Right,

	TopLeft = Top | Left,
	TopCentre = Top | HorizontalCentre,
	TopRight = Top | Right,

	CentreLeft = VerticalCentre | Left,
	Centre = VerticalCentre | HorizontalCentre,
	CentreRight = VerticalCentre | Right
}