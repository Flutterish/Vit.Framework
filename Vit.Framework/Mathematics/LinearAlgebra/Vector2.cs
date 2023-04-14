using System.Numerics;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Vector2<T> where T : unmanaged, INumber<T> {
	public T X;
	public T Y;

	public Vector2 ( T x, T y ) {
		X = x;
		Y = y;
	}

	public Vector2 ( T all ) {
		X = Y = all;
	}

	public override string ToString () {
		return $"[{X}; {Y}]";
	}

	public static Vector2<T> operator + ( Vector2<T> left, Vector2<T> right ) {
		return new() {
			X = left.X + right.X,
			Y = left.Y + right.Y
		};
	}
}
