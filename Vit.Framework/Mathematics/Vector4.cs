/// This file [Vector4.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.VectorTemplate and parameter 4 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics;

public struct Vector4<T> : IInterpolatable<Vector4<T>, T>, IEqualityOperators<Vector4<T>, Vector4<T>, bool>, IEquatable<Vector4<T>>, IValueSpan<T> where T : INumber<T> {
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
	
	#nullable disable
	public Vector4 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	
	public Vector4 ( IReadOnlyValueSpan<T> span ) {
		span.AsReadOnlySpan().CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly Vector4<T> UnitX = new( T.One, T.Zero, T.Zero, T.Zero );
	public static readonly Vector4<T> UnitY = new( T.Zero, T.One, T.Zero, T.Zero );
	public static readonly Vector4<T> UnitZ = new( T.Zero, T.Zero, T.One, T.Zero );
	public static readonly Vector4<T> UnitW = new( T.Zero, T.Zero, T.Zero, T.One );
	public static readonly Vector4<T> One = new( T.One );
	public static readonly Vector4<T> Zero = new( T.Zero );
	
	public Vector2<T> YX => new( Y, X );
	public Vector3<T> ZYX => new( Z, Y, X );
	public Vector3<T> WYX => new( W, Y, X );
	public Vector2<T> ZX => new( Z, X );
	public Vector3<T> YZX => new( Y, Z, X );
	public Vector3<T> WZX => new( W, Z, X );
	public Vector2<T> WX => new( W, X );
	public Vector3<T> YWX => new( Y, W, X );
	public Vector3<T> ZWX => new( Z, W, X );
	public Vector2<T> XY => new( X, Y );
	public Vector3<T> ZXY => new( Z, X, Y );
	public Vector3<T> WXY => new( W, X, Y );
	public Vector2<T> ZY => new( Z, Y );
	public Vector3<T> XZY => new( X, Z, Y );
	public Vector3<T> WZY => new( W, Z, Y );
	public Vector2<T> WY => new( W, Y );
	public Vector3<T> XWY => new( X, W, Y );
	public Vector3<T> ZWY => new( Z, W, Y );
	public Vector2<T> XZ => new( X, Z );
	public Vector3<T> YXZ => new( Y, X, Z );
	public Vector3<T> WXZ => new( W, X, Z );
	public Vector2<T> YZ => new( Y, Z );
	public Vector3<T> XYZ => new( X, Y, Z );
	public Vector3<T> WYZ => new( W, Y, Z );
	public Vector2<T> WZ => new( W, Z );
	public Vector3<T> XWZ => new( X, W, Z );
	public Vector3<T> YWZ => new( Y, W, Z );
	public Vector2<T> XW => new( X, W );
	public Vector3<T> YXW => new( Y, X, W );
	public Vector3<T> ZXW => new( Z, X, W );
	public Vector2<T> YW => new( Y, W );
	public Vector3<T> XYW => new( X, Y, W );
	public Vector3<T> ZYW => new( Z, Y, W );
	public Vector2<T> ZW => new( Z, W );
	public Vector3<T> XZW => new( X, Z, W );
	public Vector3<T> YZW => new( Y, Z, W );
	
	public T LengthSquared => X * X + Y * Y + Z * Z + W * W;
	
	public Vector4<T> RotatedByXY => new( -Y, X, Z, W );
	public Vector4<T> RotatedByYX => new( Y, -X, Z, W );
	public Vector4<T> RotatedByXZ => new( -Z, Y, X, W );
	public Vector4<T> RotatedByZX => new( Z, Y, -X, W );
	public Vector4<T> RotatedByXW => new( -W, Y, Z, X );
	public Vector4<T> RotatedByWX => new( W, Y, Z, -X );
	public Vector4<T> RotatedByYZ => new( X, -Z, Y, W );
	public Vector4<T> RotatedByZY => new( X, Z, -Y, W );
	public Vector4<T> RotatedByYW => new( X, -W, Z, Y );
	public Vector4<T> RotatedByWY => new( X, W, Z, -Y );
	public Vector4<T> RotatedByZW => new( X, Y, -W, Z );
	public Vector4<T> RotatedByWZ => new( X, Y, W, -Z );
	
	public Mathematics.LinearAlgebra.Generic.Vector<T> AsUnsized () => new( AsSpan() );
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref X, 4 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref X, 4 );
	
	public Vector4<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			X = Y.CreateChecked( X ),
			Y = Y.CreateChecked( this.Y ),
			Z = Y.CreateChecked( Z ),
			W = Y.CreateChecked( W )
		};
	}
	
	public T Dot ( Vector4<T> other )
		=> Dot( this, other );
	public static T Dot ( Vector4<T> left, Vector4<T> right ) {
		return left.X * right.X
			+ left.Y * right.Y
			+ left.Z * right.Z
			+ left.W * right.W;
	}
	
	public Point4<T> FromOrigin () {
		return new() {
			X = X,
			Y = Y,
			Z = Z,
			W = W
		};
	}
	
	public Vector4<T> Lerp ( Vector4<T> goal, T time ) {
		return new() {
			X = X.Lerp( goal.X, time ),
			Y = Y.Lerp( goal.Y, time ),
			Z = Z.Lerp( goal.Z, time ),
			W = W.Lerp( goal.W, time )
		};
	}
	
	public static Vector4<T> operator + ( Vector4<T> left, Vector4<T> right ) {
		return new() {
			X = left.X + right.X,
			Y = left.Y + right.Y,
			Z = left.Z + right.Z,
			W = left.W + right.W
		};
	}
	
	public static Vector4<T> operator - ( Vector4<T> left, Vector4<T> right ) {
		return new() {
			X = left.X - right.X,
			Y = left.Y - right.Y,
			Z = left.Z - right.Z,
			W = left.W - right.W
		};
	}
	
	public static Vector4<T> operator - ( Vector4<T> vector ) {
		return new( -vector.X, -vector.Y, -vector.Z, -vector.W );
	}
	
	public static Vector4<T> operator * ( Vector4<T> vector, T scale ) {
		return new() {
			X = vector.X * scale,
			Y = vector.Y * scale,
			Z = vector.Z * scale,
			W = vector.W * scale
		};
	}
	
	public static Vector4<T> operator * ( T scale, Vector4<T> vector ) {
		return new() {
			X = scale * vector.X,
			Y = scale * vector.Y,
			Z = scale * vector.Z,
			W = scale * vector.W
		};
	}
	
	public static Vector4<T> operator / ( Vector4<T> vector, T divisor ) {
		return new() {
			X = vector.X / divisor,
			Y = vector.Y / divisor,
			Z = vector.Z / divisor,
			W = vector.W / divisor
		};
	}
	
	public static bool operator == ( Vector4<T> left, Vector4<T> right ) {
		return left.X == right.X
			&& left.Y == right.Y
			&& left.Z == right.Z
			&& left.W == right.W;
	}
	
	public static bool operator != ( Vector4<T> left, Vector4<T> right ) {
		return left.X != right.X
			|| left.Y != right.Y
			|| left.Z != right.Z
			|| left.W != right.W;
	}
	public static implicit operator Vector4<T> ( (T, T, T, T) value )
		=> new( value.Item1, value.Item2, value.Item3, value.Item4 );
	
	public void Deconstruct ( out T x, out T y, out T z, out T w ) {
		x = X;
		y = Y;
		z = Z;
		w = W;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is Vector4<T> axes && Equals( axes );
	}
	
	public bool Equals ( Vector4<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( X, Y, Z, W );
	}
	
	public override string ToString () {
		return $"[{X}, {Y}, {Z}, {W}]";
	}
}

public static class Vector4Extensions {
	public static T GetLength<T> ( this Vector4<T> vector ) where T : IFloatingPointIeee754<T> {
		return T.Sqrt( vector.LengthSquared );
	}
	
	public static Vector4<T> Normalized<T> ( this Vector4<T> vector ) where T : IFloatingPointIeee754<T> {
		return vector / vector.GetLength();
	}
	
	public static void Normalize<T> ( this ref Vector4<T> vector ) where T : IFloatingPointIeee754<T> {
		var scale = T.MultiplicativeIdentity / vector.GetLength();
		vector.X *= scale;
		vector.Y *= scale;
		vector.Z *= scale;
		vector.W *= scale;
	}
	
	public static T GetLengthFast<T> ( this Vector4<T> vector ) where T : IFloatingPointIeee754<T> {
		return T.MultiplicativeIdentity / T.ReciprocalSqrtEstimate( vector.LengthSquared );
	}
	
	public static Vector4<T> NormalizedFast<T> ( this Vector4<T> vector ) where T : IFloatingPointIeee754<T> {
		return vector * T.ReciprocalSqrtEstimate( vector.LengthSquared );
	}
	
	public static void NormalizeFast<T> ( this ref Vector4<T> vector ) where T : IFloatingPointIeee754<T> {
		var scale = T.ReciprocalSqrtEstimate( vector.LengthSquared );
		vector.X *= scale;
		vector.Y *= scale;
		vector.Z *= scale;
		vector.W *= scale;
	}
}
