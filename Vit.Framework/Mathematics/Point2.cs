using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Point2<T> where T : unmanaged, INumber<T> {
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
}
