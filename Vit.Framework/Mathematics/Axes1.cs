/// This file [Axes1.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.AxesTemplate and parameter 1 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;

namespace Vit.Framework.Mathematics;

public struct Axes1<T> : IInterpolatable<Axes1<T>, T>, IEqualityOperators<Axes1<T>, Axes1<T>, bool>, IEquatable<Axes1<T>> where T : INumber<T> {
	public T X;
	
	public Axes1 ( T x ) {
		X = x;
	}
	
	#nullable disable
	public Axes1 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly Axes1<T> UnitX = new( T.One );
	public static readonly Axes1<T> One = new( T.One );
	public static readonly Axes1<T> Zero = new( T.Zero );
	
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref X, 1 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref X, 1 );
	
	public Axes1<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			X = Y.CreateChecked( X )
		};
	}
	
	public Axes1<T> Lerp ( Axes1<T> goal, T time ) {
		return new() {
			X = X.Lerp( goal.X, time )
		};
	}
	
	public static bool operator == ( Axes1<T> left, Axes1<T> right ) {
		return left.X == right.X;
	}
	
	public static bool operator != ( Axes1<T> left, Axes1<T> right ) {
		return left.X != right.X;
	}
	
	public static implicit operator Span<T> ( Axes1<T> value )
		=> value.AsSpan();
	public static implicit operator ReadOnlySpan<T> ( Axes1<T> value )
		=> value.AsReadOnlySpan();
	public static implicit operator Axes1<T> ( T value )	=> new( value );
	public void Deconstruct ( out T x ) {
		x = X;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is Axes1<T> axes && Equals( axes );
	}
	
	public bool Equals ( Axes1<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( X );
	}
	
	public override string ToString () {
		return $"<{X}>";
	}
}
