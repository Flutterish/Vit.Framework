/// This file [Vector3.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.VectorTemplate and parameter 3 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics;

public struct Vector3<T> : IInterpolatable<Vector3<T>, T>, IEqualityOperators<Vector3<T>, Vector3<T>, bool>, IEquatable<Vector3<T>>, IValueSpan<T> where T : INumber<T> {
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
	
	#nullable disable
	public Vector3 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	
	public Vector3 ( IReadOnlyValueSpan<T> span ) {
		span.AsReadOnlySpan().CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly Vector3<T> UnitX = new( T.One, T.Zero, T.Zero );
	public static readonly Vector3<T> UnitY = new( T.Zero, T.One, T.Zero );
	public static readonly Vector3<T> UnitZ = new( T.Zero, T.Zero, T.One );
	public static readonly Vector3<T> One = new( T.One );
	public static readonly Vector3<T> Zero = new( T.Zero );
	
	public Vector2<T> YX => new( Y, X );
	public Vector2<T> ZX => new( Z, X );
	public Vector2<T> XY => new( X, Y );
	public Vector2<T> ZY => new( Z, Y );
	public Vector2<T> XZ => new( X, Z );
	public Vector2<T> YZ => new( Y, Z );
	
	public T LengthSquared => X * X + Y * Y + Z * Z;
	
	public Vector3<T> RotatedByXY => new( -Y, X, Z );
	public Vector3<T> RotatedByYX => new( Y, -X, Z );
	public Vector3<T> RotatedByXZ => new( -Z, Y, X );
	public Vector3<T> RotatedByZX => new( Z, Y, -X );
	public Vector3<T> RotatedByYZ => new( X, -Z, Y );
	public Vector3<T> RotatedByZY => new( X, Z, -Y );
	
	public Mathematics.LinearAlgebra.Generic.Vector<T> AsUnsized () => new( AsSpan() );
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref X, 3 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref X, 3 );
	
	public Vector3<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			X = Y.CreateChecked( X ),
			Y = Y.CreateChecked( this.Y ),
			Z = Y.CreateChecked( Z )
		};
	}
	
	public T Dot ( Vector3<T> other )
		=> Dot( this, other );
	public static T Dot ( Vector3<T> left, Vector3<T> right ) {
		return left.X * right.X
			+ left.Y * right.Y
			+ left.Z * right.Z;
	}
	
	public Vector3<T> Cross ( Vector3<T> other )
		=> Cross( this, other );
	public static Vector3<T> Cross ( Vector3<T> left, Vector3<T> right ) {
		return new() {
			X = left.Y * right.Z - left.Z * right.Y,
			Y = left.Z * right.X - left.X * right.Z,
			Z = left.X * right.Y - left.Y * right.X
		};
	}
	
	public Point3<T> FromOrigin () {
		return new() {
			X = X,
			Y = Y,
			Z = Z
		};
	}
	
	public Vector3<T> Lerp ( Vector3<T> goal, T time ) {
		return new() {
			X = X.Lerp( goal.X, time ),
			Y = Y.Lerp( goal.Y, time ),
			Z = Z.Lerp( goal.Z, time )
		};
	}
	
	public static Vector3<T> operator + ( Vector3<T> left, Vector3<T> right ) {
		return new() {
			X = left.X + right.X,
			Y = left.Y + right.Y,
			Z = left.Z + right.Z
		};
	}
	
	public static Vector3<T> operator - ( Vector3<T> left, Vector3<T> right ) {
		return new() {
			X = left.X - right.X,
			Y = left.Y - right.Y,
			Z = left.Z - right.Z
		};
	}
	
	public static Vector3<T> operator - ( Vector3<T> vector ) {
		return new( -vector.X, -vector.Y, -vector.Z );
	}
	
	public static Vector3<T> operator * ( Vector3<T> vector, T scale ) {
		return new() {
			X = vector.X * scale,
			Y = vector.Y * scale,
			Z = vector.Z * scale
		};
	}
	
	public static Vector3<T> operator * ( T scale, Vector3<T> vector ) {
		return new() {
			X = scale * vector.X,
			Y = scale * vector.Y,
			Z = scale * vector.Z
		};
	}
	
	public static Vector3<T> operator / ( Vector3<T> vector, T divisor ) {
		return new() {
			X = vector.X / divisor,
			Y = vector.Y / divisor,
			Z = vector.Z / divisor
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
	public static implicit operator Vector3<T> ( (T, T, T) value )
		=> new( value.Item1, value.Item2, value.Item3 );
	
	public void Deconstruct ( out T x, out T y, out T z ) {
		x = X;
		y = Y;
		z = Z;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is Vector3<T> axes && Equals( axes );
	}
	
	public bool Equals ( Vector3<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( X, Y, Z );
	}
	
	public override string ToString () {
		return $"[{X}, {Y}, {Z}]";
	}
}

public static class Vector3Extensions {
	public static T GetLength<T> ( this Vector3<T> vector ) where T : IFloatingPointIeee754<T> {
		return T.Sqrt( vector.LengthSquared );
	}
	
	public static Vector3<T> Normalized<T> ( this Vector3<T> vector ) where T : IFloatingPointIeee754<T> {
		return vector / vector.GetLength();
	}
	
	public static void Normalize<T> ( this ref Vector3<T> vector ) where T : IFloatingPointIeee754<T> {
		var scale = T.MultiplicativeIdentity / vector.GetLength();
		vector.X *= scale;
		vector.Y *= scale;
		vector.Z *= scale;
	}
	
	public static T GetLengthFast<T> ( this Vector3<T> vector ) where T : IFloatingPointIeee754<T> {
		return T.MultiplicativeIdentity / T.ReciprocalSqrtEstimate( vector.LengthSquared );
	}
	
	public static Vector3<T> NormalizedFast<T> ( this Vector3<T> vector ) where T : IFloatingPointIeee754<T> {
		return vector * T.ReciprocalSqrtEstimate( vector.LengthSquared );
	}
	
	public static void NormalizeFast<T> ( this ref Vector3<T> vector ) where T : IFloatingPointIeee754<T> {
		var scale = T.ReciprocalSqrtEstimate( vector.LengthSquared );
		vector.X *= scale;
		vector.Y *= scale;
		vector.Z *= scale;
	}
}
