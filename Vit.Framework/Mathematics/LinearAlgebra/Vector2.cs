using System.Numerics;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Vector2<T> where T : unmanaged, INumber<T> {
	public T X;
	public T Y;

	public Vector2 ( T x, T y ) {
		X = x;
		Y = y;
	}
}
