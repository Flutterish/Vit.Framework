/// This file [Axes2.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.AxesTemplate and parameter 2 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics;

public struct Axes2<T> : IInterpolatable<Axes2<T>, T>, IEqualityOperators<Axes2<T>, Axes2<T>, bool>, IEquatable<Axes2<T>>, IValueSpan<T> where T : INumber<T> {
	public T X;
	public T Y;
	
	public Axes2 ( T x, T y ) {
		X = x;
		Y = y;
	}
	
	public Axes2 ( T all ) {
		X = Y = all;
	}
	
	#nullable disable
	public Axes2 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	
	public Axes2 ( IReadOnlyValueSpan<T> span ) {
		span.AsReadOnlySpan().CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly Axes2<T> UnitX = new( T.One, T.Zero );
	public static readonly Axes2<T> UnitY = new( T.Zero, T.One );
	public static readonly Axes2<T> One = new( T.One );
	public static readonly Axes2<T> Zero = new( T.Zero );
	
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref X, 2 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref X, 2 );
	
	public Axes2<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			X = Y.CreateChecked( X ),
			Y = Y.CreateChecked( this.Y )
		};
	}
	
	public Axes2<T> Lerp ( Axes2<T> goal, T time ) {
		return new() {
			X = X.Lerp( goal.X, time ),
			Y = Y.Lerp( goal.Y, time )
		};
	}
	
	public static Axes2<T> operator + ( Axes2<T> left, Axes2<T> right ) {
		return new() {
			X = left.X + right.X,
			Y = left.Y + right.Y
		};
	}
	
	public static Axes2<T> operator - ( Axes2<T> left, Axes2<T> right ) {
		return new() {
			X = left.X - right.X,
			Y = left.Y - right.Y
		};
	}
	
	public static Axes2<T> operator - ( Axes2<T> axes ) {
		return new( -axes.X, -axes.Y );
	}
	
	public static Axes2<T> operator * ( Axes2<T> axes, T scale ) {
		return new() {
			X = axes.X * scale,
			Y = axes.Y * scale
		};
	}
	
	public static Axes2<T> operator * ( T scale, Axes2<T> axes ) {
		return new() {
			X = scale * axes.X,
			Y = scale * axes.Y
		};
	}
	
	public static Axes2<T> operator / ( Axes2<T> axes, T divisor ) {
		return new() {
			X = axes.X / divisor,
			Y = axes.Y / divisor
		};
	}
	
	public static bool operator == ( Axes2<T> left, Axes2<T> right ) {
		return left.X == right.X
			&& left.Y == right.Y;
	}
	
	public static bool operator != ( Axes2<T> left, Axes2<T> right ) {
		return left.X != right.X
			|| left.Y != right.Y;
	}
	public static implicit operator Axes2<T> ( (T, T) value )
		=> new( value.Item1, value.Item2 );
	
	public void Deconstruct ( out T x, out T y ) {
		x = X;
		y = Y;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is Axes2<T> axes && Equals( axes );
	}
	
	public bool Equals ( Axes2<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( X, Y );
	}
	
	public override string ToString () {
		return $"<{X}, {Y}>";
	}
}
