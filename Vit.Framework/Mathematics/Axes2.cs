using System.Numerics;

namespace Vit.Framework.Mathematics;

public struct Axes2<T> : IEqualityOperators<Axes2<T>, Axes2<T>, bool> where T : INumber<T> {
	public T X;
	public T Y;

	public Axes2 ( T x, T y ) {
		X = x;
		Y = y;
	}

	public Axes2 ( T both ) {
		X = Y = both;
	}

	public static readonly Axes2<T> UnitX = new Axes2<T>( T.One, T.Zero );
	public static readonly Axes2<T> UnitY = new Axes2<T>( T.Zero, T.One );
	public static readonly Axes2<T> One = new Axes2<T>( T.One );
	public static readonly Axes2<T> Zero = new Axes2<T>( T.Zero );

	public static bool operator == ( Axes2<T> left, Axes2<T> right ) {
		return left.X == right.X
			&& left.Y == right.Y;
	}

	public static bool operator != ( Axes2<T> left, Axes2<T> right ) {
		return left.X != right.X
			|| left.Y != right.Y;
	}
}
