using System.Numerics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.Layout;

public struct RelativeAxes2<T> : IInterpolatable<RelativeAxes2<T>, T>, IEquatable<RelativeAxes2<T>> where T : INumber<T> {
	public LayoutUnit<T> X;
	public LayoutUnit<T> Y;

	public RelativeAxes2 ( LayoutUnit<T> x, LayoutUnit<T> y ) {
		X = x;
		Y = y;
	}

	public static Point2<T> operator * ( Size2<T> availableSpace, RelativeAxes2<T> axes ) => new() {
		X = axes.X.GetValue( availableSpace.Width ),
		Y = axes.Y.GetValue( availableSpace.Height )
	};

	public FlowAxes2<T> ToFlow ( FlowDirection direction, Size2<T> availableSpace ) {
		var point = direction.ToFlow( availableSpace * this, direction.ToFlow( availableSpace ) );
		return new( point );
	}

	public static implicit operator RelativeAxes2<T> ( ValueTuple<T, T> absolute ) => new() {
		X = absolute.Item1,
		Y = absolute.Item2
	};
	public static implicit operator RelativeAxes2<T> ( Point2<T> absolute ) => new() {
		X = absolute.X,
		Y = absolute.Y
	};

	public static implicit operator RelativeAxes2<T> ( Anchor anchor ) => new() {
		X = anchor.HasFlag( Anchor.HorizontalCentre ) ? (T.One / (T.One + T.One)).Relative() : anchor.HasFlag( Anchor.Right ) ? T.One.Relative() : T.Zero,
		Y = anchor.HasFlag( Anchor.VerticalCentre ) ? (T.One / (T.One + T.One)).Relative() : anchor.HasFlag( Anchor.Top ) ? T.One.Relative() : T.Zero
	};

	public static RelativeAxes2<T> operator + ( RelativeAxes2<T> left, RelativeAxes2<T> right ) {
		return new() {
			X = left.X + right.X,
			Y = left.Y + right.Y
		};
	}

	public static RelativeAxes2<T> operator - ( RelativeAxes2<T> left, RelativeAxes2<T> right ) {
		return new() {
			X = left.X - right.X,
			Y = left.Y - right.Y
		};
	}

	public static bool operator == ( RelativeAxes2<T> left, RelativeAxes2<T> right ) {
		return left.X == right.X
			&& left.Y == right.Y;
	}

	public static bool operator != ( RelativeAxes2<T> left, RelativeAxes2<T> right ) {
		return left.X != right.X
			|| left.Y != right.Y;
	}

	public override string ToString () {
		return $"({X}, {Y})";
	}

	public override bool Equals ( object? obj ) {
		return obj is RelativeAxes2<T> point && Equals( point );
	}

	public bool Equals ( RelativeAxes2<T> other ) {
		return this == other;
	}

	public override int GetHashCode () {
		return HashCode.Combine( X, Y );
	}

	public RelativeAxes2<T> Lerp ( RelativeAxes2<T> goal, T time ) {
		return new() {
			X = X.Lerp( goal.X, time ),
			Y = Y.Lerp( goal.Y, time )
		};
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

public static class Anchor<T> where T : INumber<T> {
	public static RelativeAxes2<T> BottomLeft => Anchor.BottomLeft;
	public static RelativeAxes2<T> BottomCentre => Anchor.BottomCentre;
	public static RelativeAxes2<T> BottomRight => Anchor.BottomRight;

	public static RelativeAxes2<T> TopLeft => Anchor.TopLeft;
	public static RelativeAxes2<T> TopCentre => Anchor.TopCentre;
	public static RelativeAxes2<T> TopRight => Anchor.TopRight;

	public static RelativeAxes2<T> CentreLeft => Anchor.CentreLeft;
	public static RelativeAxes2<T> Centre => Anchor.Centre;
	public static RelativeAxes2<T> CentreRight => Anchor.CentreRight;
}