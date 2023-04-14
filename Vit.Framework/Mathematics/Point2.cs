using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Point2<T> : IInterpolatable<Point2<T>, T> where T : INumber<T> {
	public T X;
	public T Y;

	public Point2 ( T x, T y ) {
		X = x;
		Y = y;
	}

	public static Point2<T> operator + ( Point2<T> point, Vector2<T> delta ) {
		return new() {
			X = point.X + delta.X,
			Y = point.Y + delta.Y
		};
	}

	public static Vector2<T> operator - ( Point2<T> left, Point2<T> right ) {
		return new() {
			X = left.X - right.X,
			Y = left.Y - right.Y
		};
	}

	public static Point2<T> operator - ( Point2<T> left, Vector2<T> right ) {
		return new() {
			X = left.X - right.X,
			Y = left.Y - right.Y
		};
	}

	public Point2<T> Lerp ( Point2<T> goal, T time ) {
		return new Point2<T> {
			X = X.Lerp( goal.X, time ),
			Y = Y.Lerp( goal.Y, time )
		};
	}
}
