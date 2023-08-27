/// This file [AxisAlignedBox1.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.AxisAlignedBoxTemplate and parameter 1 (System.Int32)
using System.Numerics;

namespace Vit.Framework.Mathematics;

public struct AxisAlignedBox1<T> where T : INumber<T> {
	public T MinX;
	public T MaxX;
	
	public T Width => MaxX - MinX;
	
	public Point1<T> Position => new( MinX );
	public Size1<T> Size => new( Width );
	
	public AxisAlignedBox1 ( Size1<T> size ) {
		MinX = T.Zero;
		MaxX = size.Width;
	}
	
	public AxisAlignedBox1 ( Point1<T> position, Size1<T> size ) {
		MinX = position.X;
		MaxX = MinX + size.Width;
	}
	
	public AxisAlignedBox1 ( T x, T width ) {
		MinX = x;
		MaxX = MinX + width;
	}
	
	public AxisAlignedBox1<T> Contain ( AxisAlignedBox1<T> other ) {
		return new() {
			MinX = T.Min( MinX, other.MinX ),
			MaxX = T.Max( MaxX, other.MaxX )
		};
	}
	
	public static implicit operator AxisAlignedBox1<T> ( Size1<T> size ) => new( size );
	
	public static AxisAlignedBox1<T> operator + ( AxisAlignedBox1<T> left, Vector1<T> right ) => new() {
		MinX = left.MinX + right.X,
		MaxX = left.MaxX + right.X
	};
	
	public static AxisAlignedBox1<T> operator - ( AxisAlignedBox1<T> left, Vector1<T> right ) => new() {
		MinX = left.MinX - right.X,
		MaxX = left.MaxX - right.X
	};
	
	public override string ToString () {
		return $"{Size} at {Position}";
	}
}

public static class AABox1<T> where T : INumber<T>, IFloatingPointIeee754<T> {
	public static AxisAlignedBox1<T> Undefined { get; } = new() {
		MinX = T.PositiveInfinity,
		MaxX = T.NegativeInfinity
	};
}
