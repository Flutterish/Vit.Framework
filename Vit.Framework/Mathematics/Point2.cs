/// This file [Point2.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.PointTemplate and parameter 2 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Point2<T> : IInterpolatable<Point2<T>, T>, IEqualityOperators<Point2<T>, Point2<T>, bool>, IEquatable<Point2<T>> where T : INumber<T> {
	public T X;
	public T Y;
	
	public Point2 ( T x, T y ) {
		X = x;
		Y = y;
	}
	
	public Point2 ( T all ) {
		X = Y = all;
	}
	
	#nullable disable
	public Point2 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly Point2<T> UnitX = new( T.One, T.Zero );
	public static readonly Point2<T> UnitY = new( T.Zero, T.One );
	public static readonly Point2<T> One = new( T.One );
	public static readonly Point2<T> Zero = new( T.Zero );
	
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref X, 2 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref X, 2 );
	
	public Point2<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			X = Y.CreateChecked( X ),
			Y = Y.CreateChecked( this.Y )
		};
	}
	
	public Vector2<T> FromOrigin () {
		return new() {
			X = X,
			Y = Y
		};
	}
	
	public Vector2<T> ToOrigin () {
		return new() {
			X = -X,
			Y = -Y
		};
	}
	
	public Point2<T> ScaleAboutOrigin ( T scale ) {
		return new() {
			X = X * scale,
			Y = Y * scale
		};
	}
	
	public Point2<T> ReflectAboutOrigin () {
		return new() {
			X = -X,
			Y = -Y
		};
	}
	
	public Point2<T> Lerp ( Point2<T> goal, T time ) {
		return new() {
			X = X.Lerp( goal.X, time ),
			Y = Y.Lerp( goal.Y, time )
		};
	}
	
	public static Point2<T> operator + ( Point2<T> point, Vector2<T> delta ) {
		return new() {
			X = point.X + delta.X,
			Y = point.Y + delta.Y
		};
	}
	
	public static Point2<T> operator - ( Point2<T> point, Vector2<T> delta ) {
		return new() {
			X = point.X - delta.X,
			Y = point.Y - delta.Y
		};
	}
	
	public static Vector2<T> operator - ( Point2<T> to, Point2<T> from ) {
		return new() {
			X = to.X - from.X,
			Y = to.Y - from.Y
		};
	}
	
	public static bool operator == ( Point2<T> left, Point2<T> right ) {
		return left.X == right.X
			&& left.Y == right.Y;
	}
	
	public static bool operator != ( Point2<T> left, Point2<T> right ) {
		return left.X != right.X
			|| left.Y != right.Y;
	}
	
	public static implicit operator Span<T> ( Point2<T> value )
		=> value.AsSpan();
	public static implicit operator ReadOnlySpan<T> ( Point2<T> value )
		=> value.AsReadOnlySpan();
	public static implicit operator Point2<T> ( (T, T) value )
		=> new( value.Item1, value.Item2 );
	
	public void Deconstruct ( out T x, out T y ) {
		x = X;
		y = Y;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is Point2<T> axes && Equals( axes );
	}
	
	public bool Equals ( Point2<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( X, Y );
	}
	
	public override string ToString () {
		return $"({X}, {Y})";
	}
}
