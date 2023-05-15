/// This file [Point1.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.PointTemplate and parameter 1 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Memory;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Point1<T> : IInterpolatable<Point1<T>, T>, IEqualityOperators<Point1<T>, Point1<T>, bool>, IEquatable<Point1<T>>, IValueSpan<T> where T : INumber<T> {
	public T X;
	
	public Point1 ( T x ) {
		X = x;
	}
	
	#nullable disable
	public Point1 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	
	public Point1 ( IReadOnlyValueSpan<T> span ) {
		span.AsReadOnlySpan().CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly Point1<T> UnitX = new( T.One );
	public static readonly Point1<T> One = new( T.One );
	public static readonly Point1<T> Zero = new( T.Zero );
	
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref X, 1 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref X, 1 );
	
	public Point1<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			X = Y.CreateChecked( X )
		};
	}
	
	public Vector1<T> FromOrigin () {
		return new() {
			X = X
		};
	}
	
	public Vector1<T> ToOrigin () {
		return new() {
			X = -X
		};
	}
	
	public Point1<T> ScaleAboutOrigin ( T scale ) {
		return new() {
			X = X * scale
		};
	}
	
	public Point1<T> ReflectAboutOrigin () {
		return new() {
			X = -X
		};
	}
	
	public Point1<T> Lerp ( Point1<T> goal, T time ) {
		return new() {
			X = X.Lerp( goal.X, time )
		};
	}
	
	public static Point1<T> operator + ( Point1<T> point, Vector1<T> delta ) {
		return new() {
			X = point.X + delta.X
		};
	}
	
	public static Point1<T> operator - ( Point1<T> point, Vector1<T> delta ) {
		return new() {
			X = point.X - delta.X
		};
	}
	
	public static Vector1<T> operator - ( Point1<T> to, Point1<T> from ) {
		return new() {
			X = to.X - from.X
		};
	}
	
	public static bool operator == ( Point1<T> left, Point1<T> right ) {
		return left.X == right.X;
	}
	
	public static bool operator != ( Point1<T> left, Point1<T> right ) {
		return left.X != right.X;
	}
	public static implicit operator Point1<T> ( T value )	=> new( value );
	public void Deconstruct ( out T x ) {
		x = X;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is Point1<T> axes && Equals( axes );
	}
	
	public bool Equals ( Point1<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( X );
	}
	
	public override string ToString () {
		return $"({X})";
	}
}
