/// This file [Size2.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.SizeTemplate and parameter 2 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;

namespace Vit.Framework.Mathematics;

public struct Size2<T> : IInterpolatable<Size2<T>, T>, IEqualityOperators<Size2<T>, Size2<T>, bool>, IEquatable<Size2<T>> where T : INumber<T> {
	public T Width;
	public T Height;
	
	public Size2 ( T width, T height ) {
		Width = width;
		Height = height;
	}
	
	public Size2 ( T all ) {
		Width = Height = all;
	}
	
	#nullable disable
	public Size2 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly Size2<T> UnitWidth = new( T.One, T.Zero );
	public static readonly Size2<T> UnitHeight = new( T.Zero, T.One );
	public static readonly Size2<T> One = new( T.One );
	public static readonly Size2<T> Zero = new( T.Zero );
	
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref Width, 2 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref Width, 2 );
	
	public Size2<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			Width = Y.CreateChecked( Width ),
			Height = Y.CreateChecked( Height )
		};
	}
	
	public Size2<T> Lerp ( Size2<T> goal, T time ) {
		return new() {
			Width = Width.Lerp( goal.Width, time ),
			Height = Height.Lerp( goal.Height, time )
		};
	}
	
	public static bool operator == ( Size2<T> left, Size2<T> right ) {
		return left.Width == right.Width
			&& left.Height == right.Height;
	}
	
	public static bool operator != ( Size2<T> left, Size2<T> right ) {
		return left.Width != right.Width
			|| left.Height != right.Height;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is Size2<T> axes && Equals( axes );
	}
	
	public bool Equals ( Size2<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( Width, Height );
	}
	
	public override string ToString () {
		return $"{Width}x{Height}";
	}
}
