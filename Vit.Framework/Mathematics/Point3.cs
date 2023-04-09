using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Point3<T> where T : unmanaged, INumber<T> {
	public T X;
	public T Y;
	public T Z;

	public Point3 ( T x, T y, T z ) {
		X = x;
		Y = y;
		Z = z;
	}

	public static Point3<T> operator + ( Point3<T> left, Vector3<T> right ) {
		return new() {
			X = left.X + right.X,
			Y = left.Y + right.Y,
			Z = left.Z + right.Z
		};
	}
}
