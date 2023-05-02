using System.Numerics;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Vector4<T> where T : INumber<T> {
	public T X;
	public T Y;
	public T Z;
	public T W;

	public Vector4 ( T x, T y, T z, T w ) {
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public Vector4 ( T all ) {
		X = Y = Z = W = all;
	}

	public T LengthSquared => X * X + Y * Y + Z * Z + W * W;

	public Point4<T> FromOrigin () {
		return new Point4<T> {
			X = X,
			Y = Y,
			Z = Z,
			W = W
		};
	}

	public Vector3<T> XYZ => new( X, Y, Z );

	public override string ToString () {
		return $"[{X}; {Y}; {Z}; {W}]";
	}
}
