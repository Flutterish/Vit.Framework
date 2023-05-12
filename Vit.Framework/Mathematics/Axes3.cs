/// This file [Axes3.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.AxesTemplate and parameter 3 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;

namespace Vit.Framework.Mathematics;

public struct Axes3<T> : IInterpolatable<Axes3<T>, T>, IEqualityOperators<Axes3<T>, Axes3<T>, bool>, IEquatable<Axes3<T>> where T : INumber<T> {
	public T X;
	public T Y;
	public T Z;
	
	public Axes3 ( T x, T y, T z ) {
		X = x;
		Y = y;
		Z = z;
	}
	
	public Axes3 ( T all ) {
		X = Y = Z = all;
	}
	
	#nullable disable
	public Axes3 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly Axes3<T> UnitX = new( T.One, T.Zero, T.Zero );
	public static readonly Axes3<T> UnitY = new( T.Zero, T.One, T.Zero );
	public static readonly Axes3<T> UnitZ = new( T.Zero, T.Zero, T.One );
	public static readonly Axes3<T> One = new( T.One );
	public static readonly Axes3<T> Zero = new( T.Zero );
	
	public Axes2<T> YX => new( Y, X );
	public Axes2<T> ZX => new( Z, X );
	public Axes2<T> XY => new( X, Y );
	public Axes2<T> ZY => new( Z, Y );
	public Axes2<T> XZ => new( X, Z );
	public Axes2<T> YZ => new( Y, Z );
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref X, 3 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref X, 3 );
	
	public Axes3<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			X = Y.CreateChecked( X ),
			Y = Y.CreateChecked( this.Y ),
			Z = Y.CreateChecked( Z )
		};
	}
	
	public Axes3<T> Lerp ( Axes3<T> goal, T time ) {
		return new() {
			X = X.Lerp( goal.X, time ),
			Y = Y.Lerp( goal.Y, time ),
			Z = Z.Lerp( goal.Z, time )
		};
	}
	
	public static bool operator == ( Axes3<T> left, Axes3<T> right ) {
		return left.X == right.X
			&& left.Y == right.Y
			&& left.Z == right.Z;
	}
	
	public static bool operator != ( Axes3<T> left, Axes3<T> right ) {
		return left.X != right.X
			|| left.Y != right.Y
			|| left.Z != right.Z;
	}
	
	public static implicit operator Span<T> ( Axes3<T> value )
		=> value.AsSpan();
	public static implicit operator ReadOnlySpan<T> ( Axes3<T> value )
		=> value.AsReadOnlySpan();
	
	public override bool Equals ( object? obj ) {
		return obj is Axes3<T> axes && Equals( axes );
	}
	
	public bool Equals ( Axes3<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( X, Y, Z );
	}
	
	public override string ToString () {
		return $"<{X}, {Y}, {Z}>";
	}
}
