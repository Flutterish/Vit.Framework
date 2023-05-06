using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Point4<T> : IInterpolatable<Point4<T>, T> where T : INumber<T> {
	public T X;
	public T Y;
	public T Z;
	public T W;

	public Point3<T> XYZ => new( X, Y, Z );

	public Point4 ( T x, T y, T z, T w ) {
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public Point4 ( T all ) {
		X = Y = Z = W = all;
	}

	public static Point4<T> operator + ( Point4<T> left, Vector4<T> right ) {
		return new() {
			X = left.X + right.X,
			Y = left.Y + right.Y,
			Z = left.Z + right.Z,
			W = left.W + right.W
		};
	}

	public static Vector4<T> operator - ( Point4<T> left, Point4<T> right ) {
		return new() {
			X = left.X - right.X,
			Y = left.Y - right.Y,
			Z = left.Z - right.Z,
			W = left.W - right.W
		};
	}

	public Vector4<T> FromOrigin () {
		return new Vector4<T> {
			X = X,
			Y = Y,
			Z = Z,
			W = W
		};
	}

	public Point4<T> Lerp ( Point4<T> goal, T time ) {
		return new Point4<T> {
			X = X.Lerp( goal.X, time ),
			Y = Y.Lerp( goal.Y, time ),
			Z = Z.Lerp( goal.Z, time ),
			W = W.Lerp( goal.W, time )
		};
	}

	public override string ToString () {
		return $"({X}, {Y}, {Z}, {W})";
	}
}
