/// This file [Vector2.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.VectorTemplate and parameter 2 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Vector2<T> : IInterpolatable<Vector2<T>, T>, IEqualityOperators<Vector2<T>, Vector2<T>, bool>, IEquatable<Vector2<T>>, IValueSpan<T> where T : INumber<T> {
	public T X;
	public T Y;
	
	public Vector2 ( T x, T y ) {
		X = x;
		Y = y;
	}
	
	public Vector2 ( T all ) {
		X = Y = all;
	}
	
	#nullable disable
	public Vector2 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	
	public Vector2 ( IReadOnlyValueSpan<T> span ) {
		span.AsReadOnlySpan().CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly Vector2<T> UnitX = new( T.One, T.Zero );
	public static readonly Vector2<T> UnitY = new( T.Zero, T.One );
	public static readonly Vector2<T> One = new( T.One );
	public static readonly Vector2<T> Zero = new( T.Zero );
	
	
	public T LengthSquared => X * X + Y * Y;
	
	public Vector2<T> LeftInXY => new( -Y, X );
	public Vector2<T> RightInXY => new( Y, -X );
	
	public Vector2<T> Left => new( -Y, X );
	public Vector2<T> Right => new( Y, -X );
	
	public Mathematics.LinearAlgebra.Generic.Vector<T> AsUnsized () => new( AsSpan() );
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref X, 2 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref X, 2 );
	
	public Vector2<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			X = Y.CreateChecked( X ),
			Y = Y.CreateChecked( this.Y )
		};
	}
	
	public T Dot ( Vector2<T> other )
		=> Dot( this, other );
	public static T Dot ( Vector2<T> left, Vector2<T> right ) {
		return left.X * right.X
			+ left.Y * right.Y;
	}
	
	public Point2<T> FromOrigin () {
		return new() {
			X = X,
			Y = Y
		};
	}
	
	public Vector2<T> Lerp ( Vector2<T> goal, T time ) {
		return new() {
			X = X.Lerp( goal.X, time ),
			Y = Y.Lerp( goal.Y, time )
		};
	}
	
	public static Vector2<T> operator + ( Vector2<T> left, Vector2<T> right ) {
		return new() {
			X = left.X + right.X,
			Y = left.Y + right.Y
		};
	}
	
	public static Vector2<T> operator - ( Vector2<T> left, Vector2<T> right ) {
		return new() {
			X = left.X - right.X,
			Y = left.Y - right.Y
		};
	}
	
	public static Vector2<T> operator - ( Vector2<T> vector ) {
		return new( -vector.X, -vector.Y );
	}
	
	public static Vector2<T> operator * ( Vector2<T> vector, T scale ) {
		return new() {
			X = vector.X * scale,
			Y = vector.Y * scale
		};
	}
	
	public static Vector2<T> operator * ( T scale, Vector2<T> vector ) {
		return new() {
			X = scale * vector.X,
			Y = scale * vector.Y
		};
	}
	
	public static Vector2<T> operator / ( Vector2<T> vector, T divisor ) {
		return new() {
			X = vector.X / divisor,
			Y = vector.Y / divisor
		};
	}
	
	public static bool operator == ( Vector2<T> left, Vector2<T> right ) {
		return left.X == right.X
			&& left.Y == right.Y;
	}
	
	public static bool operator != ( Vector2<T> left, Vector2<T> right ) {
		return left.X != right.X
			|| left.Y != right.Y;
	}
	public static implicit operator Vector2<T> ( (T, T) value )
		=> new( value.Item1, value.Item2 );
	
	public void Deconstruct ( out T x, out T y ) {
		x = X;
		y = Y;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is Vector2<T> axes && Equals( axes );
	}
	
	public bool Equals ( Vector2<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( X, Y );
	}
	
	public override string ToString () {
		return $"[{X}, {Y}]";
	}
}

public static class Vector2Extensions {
	public static T GetLength<T> ( this Vector2<T> vector ) where T : IFloatingPointIeee754<T> {
		return T.Sqrt( vector.LengthSquared );
	}
	
	public static Vector2<T> Normalized<T> ( this Vector2<T> vector ) where T : IFloatingPointIeee754<T> {
		return vector / vector.GetLength();
	}
	
	public static void Normalize<T> ( this ref Vector2<T> vector ) where T : IFloatingPointIeee754<T> {
		var scale = T.MultiplicativeIdentity / vector.GetLength();
		vector.X *= scale;
		vector.Y *= scale;
	}
	
	public static T GetLengthFast<T> ( this Vector2<T> vector ) where T : IFloatingPointIeee754<T> {
		return T.MultiplicativeIdentity / T.ReciprocalSqrtEstimate( vector.LengthSquared );
	}
	
	public static Vector2<T> NormalizedFast<T> ( this Vector2<T> vector ) where T : IFloatingPointIeee754<T> {
		return vector * T.ReciprocalSqrtEstimate( vector.LengthSquared );
	}
	
	public static void NormalizeFast<T> ( this ref Vector2<T> vector ) where T : IFloatingPointIeee754<T> {
		var scale = T.ReciprocalSqrtEstimate( vector.LengthSquared );
		vector.X *= scale;
		vector.Y *= scale;
	}
	
	public static Radians<T> GetAngle<T> ( this Vector2<T> vector ) where T : IFloatingPointIeee754<T> {
		return T.Atan2( vector.Y, vector.X ).Radians();
	}
}
