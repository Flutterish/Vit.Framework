using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Point3<T> : IInterpolatable<Point3<T>, T> where T : INumber<T> {
	public T X;
	public T Y;
	public T Z;

	public Point3 ( T x, T y, T z ) {
		X = x;
		Y = y;
		Z = z;
	}

	public Point3 ( T all ) {
		X = Y = Z = all;
	}

	public static Point3<T> operator + ( Point3<T> left, Vector3<T> right ) {
		return new() {
			X = left.X + right.X,
			Y = left.Y + right.Y,
			Z = left.Z + right.Z
		};
	}

	public static Vector3<T> operator - ( Point3<T> left, Point3<T> right ) {
		return new() {
			X = left.X - right.X,
			Y = left.Y - right.Y,
			Z = left.Z - right.Z
		};
	}

	public Vector3<T> FromOrigin () {
		return new Vector3<T> {
			X = X,
			Y = Y,
			Z = Z
		};
	}

	public Point3<T> Lerp ( Point3<T> goal, T time ) {
		return new Point3<T> {
			X = X.Lerp( goal.X, time ),
			Y = Y.Lerp( goal.Y, time ),
			Z = Z.Lerp( goal.Z, time )
		};
	}

	public Point2<T> XY => new( X, Y );

	public override string ToString () {
		return $"({X}, {Y}, {Z})";
	}
}
