using System.Numerics;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Vector2<T> where T : INumber<T> {
	public T X;
	public T Y;

	public Vector2 ( T x, T y ) {
		X = x;
		Y = y;
	}

	public Vector2 ( T all ) {
		X = Y = all;
	}

	public Vector2<T> Left => new Vector2<T>( -Y, X );
	public Vector2<T> Right => new Vector2<T>( Y, -X );

	public T LengthSquared => X * X + Y * Y;

	public static readonly Vector2<T> UnitX = new Vector2<T>( T.One, T.Zero );
	public static readonly Vector2<T> UnitY = new Vector2<T>( T.Zero, T.One );
	public static readonly Vector2<T> One = new Vector2<T>( T.One );
	public static readonly Vector2<T> Zero = new Vector2<T>( T.Zero );

	public override string ToString () {
		return $"[{X}; {Y}]";
	}

	public static Vector2<T> operator + ( Vector2<T> left, Vector2<T> right ) {
		return new() {
			X = left.X + right.X,
			Y = left.Y + right.Y
		};
	}

	public static Vector2<T> operator / ( Vector2<T> left, T right ) {
		return new() {
			X = left.X / right,
			Y = left.Y / right
		};
	}

	public static Vector2<T> operator * ( Vector2<T> left, T right ) {
		return new() {
			X = left.X * right,
			Y = left.Y * right
		};
	}
}

public static class Vector2Extensions {
	public static T GetLength<T> ( this Vector2<T> vector ) where T : unmanaged, IFloatingPointIeee754<T> {
		return T.Sqrt( vector.LengthSquared );
	}

	public static Vector2<T> Normalized<T> ( this Vector2<T> vector ) where T : unmanaged, IFloatingPointIeee754<T> {
		return vector / vector.GetLength();
	}

	public static void Normalize<T> ( this ref Vector2<T> vector ) where T : unmanaged, IFloatingPointIeee754<T> {
		var scale = T.MultiplicativeIdentity / vector.GetLength();
		vector.X *= scale;
		vector.Y *= scale;
	}

	public static T GetLengthFast<T> ( this Vector2<T> vector ) where T : unmanaged, IFloatingPointIeee754<T> {
		return T.MultiplicativeIdentity / T.ReciprocalSqrtEstimate( vector.LengthSquared );
	}

	public static Vector2<T> NormalizedFast<T> ( this Vector2<T> vector ) where T : unmanaged, IFloatingPointIeee754<T> {
		return vector * T.ReciprocalSqrtEstimate( vector.LengthSquared );
	}
	public static void NormalizeFast<T> ( this ref Vector2<T> vector ) where T : unmanaged, IFloatingPointIeee754<T> {
		var scale = T.ReciprocalSqrtEstimate( vector.LengthSquared );
		vector.X *= scale;
		vector.Y *= scale;
	}

	public static Radians<T> GetAngle<T> ( this Vector2<T> vector ) where T : unmanaged, IFloatingPointIeee754<T> {
		return T.Atan2( vector.Y, vector.X ).Radians();
	}
}