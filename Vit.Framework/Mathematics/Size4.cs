/// This file [Size4.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.SizeTemplate and parameter 4 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;

namespace Vit.Framework.Mathematics;

public struct Size4<T> : IInterpolatable<Size4<T>, T>, IEqualityOperators<Size4<T>, Size4<T>, bool>, IEquatable<Size4<T>> where T : INumber<T> {
	public T Width;
	public T Height;
	public T Depth;
	public T Anakata;
	
	public Size4 ( T width, T height, T depth, T anakata ) {
		Width = width;
		Height = height;
		Depth = depth;
		Anakata = anakata;
	}
	
	public Size4 ( T all ) {
		Width = Height = Depth = Anakata = all;
	}
	
	#nullable disable
	public Size4 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly Size4<T> UnitWidth = new( T.One, T.Zero, T.Zero, T.Zero );
	public static readonly Size4<T> UnitHeight = new( T.Zero, T.One, T.Zero, T.Zero );
	public static readonly Size4<T> UnitDepth = new( T.Zero, T.Zero, T.One, T.Zero );
	public static readonly Size4<T> UnitAnakata = new( T.Zero, T.Zero, T.Zero, T.One );
	public static readonly Size4<T> One = new( T.One );
	public static readonly Size4<T> Zero = new( T.Zero );
	
	public Size2<T> HeightWidth => new( Height, Width );
	public Size3<T> DepthHeightWidth => new( Depth, Height, Width );
	public Size3<T> AnakataHeightWidth => new( Anakata, Height, Width );
	public Size2<T> DepthWidth => new( Depth, Width );
	public Size3<T> HeightDepthWidth => new( Height, Depth, Width );
	public Size3<T> AnakataDepthWidth => new( Anakata, Depth, Width );
	public Size2<T> AnakataWidth => new( Anakata, Width );
	public Size3<T> HeightAnakataWidth => new( Height, Anakata, Width );
	public Size3<T> DepthAnakataWidth => new( Depth, Anakata, Width );
	public Size2<T> WidthHeight => new( Width, Height );
	public Size3<T> DepthWidthHeight => new( Depth, Width, Height );
	public Size3<T> AnakataWidthHeight => new( Anakata, Width, Height );
	public Size2<T> DepthHeight => new( Depth, Height );
	public Size3<T> WidthDepthHeight => new( Width, Depth, Height );
	public Size3<T> AnakataDepthHeight => new( Anakata, Depth, Height );
	public Size2<T> AnakataHeight => new( Anakata, Height );
	public Size3<T> WidthAnakataHeight => new( Width, Anakata, Height );
	public Size3<T> DepthAnakataHeight => new( Depth, Anakata, Height );
	public Size2<T> WidthDepth => new( Width, Depth );
	public Size3<T> HeightWidthDepth => new( Height, Width, Depth );
	public Size3<T> AnakataWidthDepth => new( Anakata, Width, Depth );
	public Size2<T> HeightDepth => new( Height, Depth );
	public Size3<T> WidthHeightDepth => new( Width, Height, Depth );
	public Size3<T> AnakataHeightDepth => new( Anakata, Height, Depth );
	public Size2<T> AnakataDepth => new( Anakata, Depth );
	public Size3<T> WidthAnakataDepth => new( Width, Anakata, Depth );
	public Size3<T> HeightAnakataDepth => new( Height, Anakata, Depth );
	public Size2<T> WidthAnakata => new( Width, Anakata );
	public Size3<T> HeightWidthAnakata => new( Height, Width, Anakata );
	public Size3<T> DepthWidthAnakata => new( Depth, Width, Anakata );
	public Size2<T> HeightAnakata => new( Height, Anakata );
	public Size3<T> WidthHeightAnakata => new( Width, Height, Anakata );
	public Size3<T> DepthHeightAnakata => new( Depth, Height, Anakata );
	public Size2<T> DepthAnakata => new( Depth, Anakata );
	public Size3<T> WidthDepthAnakata => new( Width, Depth, Anakata );
	public Size3<T> HeightDepthAnakata => new( Height, Depth, Anakata );
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref Width, 4 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref Width, 4 );
	
	public Size4<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			Width = Y.CreateChecked( Width ),
			Height = Y.CreateChecked( Height ),
			Depth = Y.CreateChecked( Depth ),
			Anakata = Y.CreateChecked( Anakata )
		};
	}
	
	public Size4<T> Lerp ( Size4<T> goal, T time ) {
		return new() {
			Width = Width.Lerp( goal.Width, time ),
			Height = Height.Lerp( goal.Height, time ),
			Depth = Depth.Lerp( goal.Depth, time ),
			Anakata = Anakata.Lerp( goal.Anakata, time )
		};
	}
	
	public static bool operator == ( Size4<T> left, Size4<T> right ) {
		return left.Width == right.Width
			&& left.Height == right.Height
			&& left.Depth == right.Depth
			&& left.Anakata == right.Anakata;
	}
	
	public static bool operator != ( Size4<T> left, Size4<T> right ) {
		return left.Width != right.Width
			|| left.Height != right.Height
			|| left.Depth != right.Depth
			|| left.Anakata != right.Anakata;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is Size4<T> axes && Equals( axes );
	}
	
	public bool Equals ( Size4<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( Width, Height, Depth, Anakata );
	}
	
	public override string ToString () {
		return $"{Width}x{Height}x{Depth}x{Anakata}";
	}
}
