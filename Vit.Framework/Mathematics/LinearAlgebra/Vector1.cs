/// This file [Vector1.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.VectorTemplate and parameter 1 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Vector1<T> : IInterpolatable<Vector1<T>, T>, IEqualityOperators<Vector1<T>, Vector1<T>, bool>, IEquatable<Vector1<T>> where T : INumber<T> {
	public T X;
	
	public Vector1 ( T x ) {
		X = x;
	}
	
	#nullable disable
	public Vector1 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly Vector1<T> UnitX = new( T.One );
	public static readonly Vector1<T> One = new( T.One );
	public static readonly Vector1<T> Zero = new( T.Zero );
	
	
	public T LengthSquared => X * X;
	
	public Generic.Vector<T> AsUnsized () => new( AsSpan() );
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref X, 1 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref X, 1 );
	
	public Vector1<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			X = Y.CreateChecked( X )
		};
	}
	
	public T Dot ( Vector1<T> other )
		=> Dot( this, other );
	public static T Dot ( Vector1<T> left, Vector1<T> right ) {
		return left.X * right.X;
	}
	
	public Point1<T> FromOrigin () {
		return new() {
			X = X
		};
	}
	
	public Vector1<T> Lerp ( Vector1<T> goal, T time ) {
		return new() {
			X = X.Lerp( goal.X, time )
		};
	}
	
	public static Vector1<T> operator + ( Vector1<T> left, Vector1<T> right ) {
		return new() {
			X = left.X + right.X
		};
	}
	
	public static Vector1<T> operator - ( Vector1<T> left, Vector1<T> right ) {
		return new() {
			X = left.X - right.X
		};
	}
	
	public static Vector1<T> operator - ( Vector1<T> vector ) {
		return new( -vector.X );
	}
	
	public static Vector1<T> operator * ( Vector1<T> vector, T scale ) {
		return new() {
			X = vector.X * scale
		};
	}
	
	public static Vector1<T> operator * ( T scale, Vector1<T> vector ) {
		return new() {
			X = scale * vector.X
		};
	}
	
	public static Vector1<T> operator / ( Vector1<T> vector, T divisor ) {
		return new() {
			X = vector.X / divisor
		};
	}
	
	public static bool operator == ( Vector1<T> left, Vector1<T> right ) {
		return left.X == right.X;
	}
	
	public static bool operator != ( Vector1<T> left, Vector1<T> right ) {
		return left.X != right.X;
	}
	
	public static implicit operator Span<T> ( Vector1<T> value )
		=> value.AsSpan();
	public static implicit operator ReadOnlySpan<T> ( Vector1<T> value )
		=> value.AsReadOnlySpan();
	public static implicit operator Vector1<T> ( T value )	=> new( value );
	public void Deconstruct ( out T x ) {
		x = X;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is Vector1<T> axes && Equals( axes );
	}
	
	public bool Equals ( Vector1<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( X );
	}
	
	public override string ToString () {
		return $"[{X}]";
	}
}

public static class Vector1Extensions {
	public static T GetLength<T> ( this Vector1<T> vector ) where T : IFloatingPointIeee754<T> {
		return T.Sqrt( vector.LengthSquared );
	}
	
	public static Vector1<T> Normalized<T> ( this Vector1<T> vector ) where T : IFloatingPointIeee754<T> {
		return vector / vector.GetLength();
	}
	
	public static void Normalize<T> ( this ref Vector1<T> vector ) where T : IFloatingPointIeee754<T> {
		var scale = T.MultiplicativeIdentity / vector.GetLength();
		vector.X *= scale;
	}
	
	public static T GetLengthFast<T> ( this Vector1<T> vector ) where T : IFloatingPointIeee754<T> {
		return T.MultiplicativeIdentity / T.ReciprocalSqrtEstimate( vector.LengthSquared );
	}
	
	public static Vector1<T> NormalizedFast<T> ( this Vector1<T> vector ) where T : IFloatingPointIeee754<T> {
		return vector * T.ReciprocalSqrtEstimate( vector.LengthSquared );
	}
	
	public static void NormalizeFast<T> ( this ref Vector1<T> vector ) where T : IFloatingPointIeee754<T> {
		var scale = T.ReciprocalSqrtEstimate( vector.LengthSquared );
		vector.X *= scale;
	}
}
