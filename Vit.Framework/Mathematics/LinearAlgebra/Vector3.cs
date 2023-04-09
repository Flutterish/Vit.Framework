using System.Numerics;
using System.Runtime.InteropServices;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Vector3<T> where T : unmanaged, INumber<T> {
	public T X;
	public T Y;
	public T Z;

	public Vector3 ( T x, T y, T z ) {
		X = x;
		Y = y;
		Z = z;
	}

	public Vector3 ( T all ) {
		X = Y = Z = all;
	}

	public T LengthSquared => X * X + Y * Y + Z * Z;

	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref X, 3 );
	public Generic.Vector<T> AsUnsized () => new( AsSpan() );

	public override string ToString () {
		return $"[{X}; {Y}; {Z}]";
	}

	public static readonly Vector3<T> UnitX = new( T.One, T.Zero, T.Zero );
	public static readonly Vector3<T> UnitY = new( T.Zero, T.One, T.Zero );
	public static readonly Vector3<T> UnitZ = new( T.Zero, T.Zero, T.One );
	public static readonly Vector3<T> Zero = new( T.Zero );
	public static readonly Vector3<T> One = new( T.One );

	public Vector3<T> Lerp<TTime> ( Vector3<T> goal, TTime time ) where TTime : INumber<TTime>, IMultiplyOperators<TTime, T, T> {
		return new() {
			X = (TTime.MultiplicativeIdentity - time) * X + time * goal.X,
			Y = (TTime.MultiplicativeIdentity - time) * Y + time * goal.Y,
			Z = (TTime.MultiplicativeIdentity - time) * Z + time * goal.Z
		};
	}

	public static T Dot ( Vector3<T> left, Vector3<T> right ) {
		return left.X * right.X
			 + left.Y * right.Y
			 + left.Z * right.Z;
	}
	public T Dot ( Vector3<T> other )
		=> Dot( this, other );

	public static Vector3<T> Cross ( Vector3<T> left, Vector3<T> right ) {
		return new() {
			X = left.Y * right.Z - left.Z * right.Y,
			Y = left.Z * right.X - left.X * right.Z,
			Z = left.X * right.Y - left.Y * right.X
		};
	}
	public Vector3<T> Cross ( Vector3<T> other )
		=> Cross( this, other );

	public override bool Equals ( object? obj ) {
		return obj is Vector3<T> vector &&
				EqualityComparer<T>.Default.Equals( X, vector.X ) &&
				EqualityComparer<T>.Default.Equals( Y, vector.Y ) &&
				EqualityComparer<T>.Default.Equals( Z, vector.Z );
	}

	public override int GetHashCode () {
		return HashCode.Combine( X, Y, Z );
	}

	public static Vector3<T> operator - ( Vector3<T> left, Vector3<T> right ) {
		return new() {
			X = left.X - right.X,
			Y = left.Y - right.Y,
			Z = left.Z - right.Z
		};
	}
	public static Vector3<T> operator + ( Vector3<T> left, Vector3<T> right ) {
		return new() {
			X = left.X + right.X,
			Y = left.Y + right.Y,
			Z = left.Z + right.Z
		};
	}
	public static Vector3<T> operator / ( Vector3<T> left, T right ) {
		return new() {
			X = left.X / right,
			Y = left.Y / right,
			Z = left.Z / right
		};
	}
	public static Vector3<T> operator * ( Vector3<T> left, T right ) {
		return new() {
			X = left.X * right,
			Y = left.Y * right,
			Z = left.Z * right
		};
	}
	public static Vector3<T> operator * ( T left, Vector3<T> right ) {
		return new() {
			X = left * right.X,
			Y = left * right.Y,
			Z = left * right.Z
		};
	}
	public static Vector3<T> operator - ( Vector3<T> vector ) {
		return new() {
			X = -vector.X,
			Y = -vector.Y,
			Z = -vector.Z
		};
	}

	public static bool operator == ( Vector3<T> left, Vector3<T> right ) {
		return left.X == right.X
			&& left.Y == right.Y
			&& left.Z == right.Z;
	}

	public static bool operator != ( Vector3<T> left, Vector3<T> right ) {
		return left.X != right.X
			|| left.Y != right.Y
			|| left.Z != right.Z;
	}
}

public static class Vector3Extensions {
	public static T GetLength<T> ( this Vector3<T> vector ) where T : unmanaged, IFloatingPointIeee754<T> {
		return T.Sqrt( vector.LengthSquared );
	}

	public static Vector3<T> Normalized<T> ( this Vector3<T> vector ) where T : unmanaged, IFloatingPointIeee754<T> {
		return vector / vector.GetLength();
	}

	public static void Normalize<T> ( this Vector3<T> vector ) where T : unmanaged, IFloatingPointIeee754<T> {
		var scale = T.MultiplicativeIdentity / vector.GetLength();
		vector.X *= scale;
		vector.Y *= scale;
		vector.Z *= scale;
	}

	public static T GetLengthFast<T> ( this Vector3<T> vector ) where T : unmanaged, IFloatingPointIeee754<T> {
		return T.MultiplicativeIdentity / T.ReciprocalSqrtEstimate( vector.LengthSquared );
	}

	public static Vector3<T> NormalizedFast<T> ( this Vector3<T> vector ) where T : unmanaged, IFloatingPointIeee754<T> {
		return vector * T.ReciprocalSqrtEstimate( vector.LengthSquared );
	}
	public static void NormalizeFast<T> ( this Vector3<T> vector ) where T : unmanaged, IFloatingPointIeee754<T> {
		var scale = T.ReciprocalSqrtEstimate( vector.LengthSquared );
		vector.X *= scale;
		vector.Y *= scale;
		vector.Z *= scale;
	}
}