/// This file [Axes4.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.AxesTemplate and parameter 4 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics;

public struct Axes4<T> : IInterpolatable<Axes4<T>, T>, IEqualityOperators<Axes4<T>, Axes4<T>, bool>, IEquatable<Axes4<T>>, IValueSpan<T> where T : INumber<T> {
	public T X;
	public T Y;
	public T Z;
	public T W;
	
	public Axes4 ( T x, T y, T z, T w ) {
		X = x;
		Y = y;
		Z = z;
		W = w;
	}
	
	public Axes4 ( T all ) {
		X = Y = Z = W = all;
	}
	
	#nullable disable
	public Axes4 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	
	public Axes4 ( IReadOnlyValueSpan<T> span ) {
		span.AsReadOnlySpan().CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly Axes4<T> UnitX = new( T.One, T.Zero, T.Zero, T.Zero );
	public static readonly Axes4<T> UnitY = new( T.Zero, T.One, T.Zero, T.Zero );
	public static readonly Axes4<T> UnitZ = new( T.Zero, T.Zero, T.One, T.Zero );
	public static readonly Axes4<T> UnitW = new( T.Zero, T.Zero, T.Zero, T.One );
	public static readonly Axes4<T> One = new( T.One );
	public static readonly Axes4<T> Zero = new( T.Zero );
	
	public Axes2<T> YX => new( Y, X );
	public Axes3<T> ZYX => new( Z, Y, X );
	public Axes3<T> WYX => new( W, Y, X );
	public Axes2<T> ZX => new( Z, X );
	public Axes3<T> YZX => new( Y, Z, X );
	public Axes3<T> WZX => new( W, Z, X );
	public Axes2<T> WX => new( W, X );
	public Axes3<T> YWX => new( Y, W, X );
	public Axes3<T> ZWX => new( Z, W, X );
	public Axes2<T> XY => new( X, Y );
	public Axes3<T> ZXY => new( Z, X, Y );
	public Axes3<T> WXY => new( W, X, Y );
	public Axes2<T> ZY => new( Z, Y );
	public Axes3<T> XZY => new( X, Z, Y );
	public Axes3<T> WZY => new( W, Z, Y );
	public Axes2<T> WY => new( W, Y );
	public Axes3<T> XWY => new( X, W, Y );
	public Axes3<T> ZWY => new( Z, W, Y );
	public Axes2<T> XZ => new( X, Z );
	public Axes3<T> YXZ => new( Y, X, Z );
	public Axes3<T> WXZ => new( W, X, Z );
	public Axes2<T> YZ => new( Y, Z );
	public Axes3<T> XYZ => new( X, Y, Z );
	public Axes3<T> WYZ => new( W, Y, Z );
	public Axes2<T> WZ => new( W, Z );
	public Axes3<T> XWZ => new( X, W, Z );
	public Axes3<T> YWZ => new( Y, W, Z );
	public Axes2<T> XW => new( X, W );
	public Axes3<T> YXW => new( Y, X, W );
	public Axes3<T> ZXW => new( Z, X, W );
	public Axes2<T> YW => new( Y, W );
	public Axes3<T> XYW => new( X, Y, W );
	public Axes3<T> ZYW => new( Z, Y, W );
	public Axes2<T> ZW => new( Z, W );
	public Axes3<T> XZW => new( X, Z, W );
	public Axes3<T> YZW => new( Y, Z, W );
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref X, 4 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref X, 4 );
	
	public Axes4<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			X = Y.CreateChecked( X ),
			Y = Y.CreateChecked( this.Y ),
			Z = Y.CreateChecked( Z ),
			W = Y.CreateChecked( W )
		};
	}
	
	public Axes4<T> Lerp ( Axes4<T> goal, T time ) {
		return new() {
			X = X.Lerp( goal.X, time ),
			Y = Y.Lerp( goal.Y, time ),
			Z = Z.Lerp( goal.Z, time ),
			W = W.Lerp( goal.W, time )
		};
	}
	
	public static bool operator == ( Axes4<T> left, Axes4<T> right ) {
		return left.X == right.X
			&& left.Y == right.Y
			&& left.Z == right.Z
			&& left.W == right.W;
	}
	
	public static bool operator != ( Axes4<T> left, Axes4<T> right ) {
		return left.X != right.X
			|| left.Y != right.Y
			|| left.Z != right.Z
			|| left.W != right.W;
	}
	public static implicit operator Axes4<T> ( (T, T, T, T) value )
		=> new( value.Item1, value.Item2, value.Item3, value.Item4 );
	
	public void Deconstruct ( out T x, out T y, out T z, out T w ) {
		x = X;
		y = Y;
		z = Z;
		w = W;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is Axes4<T> axes && Equals( axes );
	}
	
	public bool Equals ( Axes4<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( X, Y, Z, W );
	}
	
	public override string ToString () {
		return $"<{X}, {Y}, {Z}, {W}>";
	}
}
