/// This file [Size3.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.SizeTemplate and parameter 3 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics;

public struct Size3<T> : IInterpolatable<Size3<T>, T>, IEqualityOperators<Size3<T>, Size3<T>, bool>, IEquatable<Size3<T>>, IValueSpan<T> where T : INumber<T> {
	public T Width;
	public T Height;
	public T Depth;
	
	public Size3 ( T width, T height, T depth ) {
		Width = width;
		Height = height;
		Depth = depth;
	}
	
	public Size3 ( T all ) {
		Width = Height = Depth = all;
	}
	
	#nullable disable
	public Size3 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	
	public Size3 ( IReadOnlyValueSpan<T> span ) {
		span.AsReadOnlySpan().CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly Size3<T> UnitWidth = new( T.One, T.Zero, T.Zero );
	public static readonly Size3<T> UnitHeight = new( T.Zero, T.One, T.Zero );
	public static readonly Size3<T> UnitDepth = new( T.Zero, T.Zero, T.One );
	public static readonly Size3<T> One = new( T.One );
	public static readonly Size3<T> Zero = new( T.Zero );
	
	public Size2<T> HeightWidth => new( Height, Width );
	public Size2<T> DepthWidth => new( Depth, Width );
	public Size2<T> WidthHeight => new( Width, Height );
	public Size2<T> DepthHeight => new( Depth, Height );
	public Size2<T> WidthDepth => new( Width, Depth );
	public Size2<T> HeightDepth => new( Height, Depth );
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref Width, 3 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref Width, 3 );
	
	public Size3<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			Width = Y.CreateChecked( Width ),
			Height = Y.CreateChecked( Height ),
			Depth = Y.CreateChecked( Depth )
		};
	}
	
	public Size3<T> Lerp ( Size3<T> goal, T time ) {
		return new() {
			Width = Width.Lerp( goal.Width, time ),
			Height = Height.Lerp( goal.Height, time ),
			Depth = Depth.Lerp( goal.Depth, time )
		};
	}
	
	public static Size3<T> operator * ( Size3<T> left, T right ) {
		return new() {
			Width = left.Width * right,
			Height = left.Height * right,
			Depth = left.Depth * right
		};
	}
	
	public static Size3<T> operator * ( T left, Size3<T> right ) {
		return new() {
			Width = left * right.Width,
			Height = left * right.Height,
			Depth = left * right.Depth
		};
	}
	
	public static Size3<T> operator / ( Size3<T> left, T right ) {
		return new() {
			Width = left.Width / right,
			Height = left.Height / right,
			Depth = left.Depth / right
		};
	}
	
	public static bool operator == ( Size3<T> left, Size3<T> right ) {
		return left.Width == right.Width
			&& left.Height == right.Height
			&& left.Depth == right.Depth;
	}
	
	public static bool operator != ( Size3<T> left, Size3<T> right ) {
		return left.Width != right.Width
			|| left.Height != right.Height
			|| left.Depth != right.Depth;
	}
	public static implicit operator Size3<T> ( (T, T, T) value )
		=> new( value.Item1, value.Item2, value.Item3 );
	
	public void Deconstruct ( out T width, out T height, out T depth ) {
		width = Width;
		height = Height;
		depth = Depth;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is Size3<T> axes && Equals( axes );
	}
	
	public bool Equals ( Size3<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( Width, Height, Depth );
	}
	
	public override string ToString () {
		return $"{Width}x{Height}x{Depth}";
	}
}
