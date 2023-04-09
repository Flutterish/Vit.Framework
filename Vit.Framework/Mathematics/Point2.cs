using System.Numerics;

namespace Vit.Framework.Mathematics;

public struct Point2<T> where T : unmanaged, INumber<T> {
	public T X;
	public T Y;

	public Point2 ( T x, T y ) {
		X = x;
		Y = y;
	}
}
