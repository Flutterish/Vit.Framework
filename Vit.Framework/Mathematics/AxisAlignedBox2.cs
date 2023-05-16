/// This file [AxisAlignedBox2.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.AxisAlignedBoxTemplate and parameter 2 (System.Int32)
using System.Numerics;

namespace Vit.Framework.Mathematics;

public struct AxisAlignedBox2<T> where T : INumber<T> {
	public T MinX;
	public T MaxX;
	public T MinY;
	public T MaxY;
	
	public T Width => MaxX - MinX;
	public T Height => MaxY - MinY;
	
	public Point2<T> Position => new( MinX, MinY );
	public Size2<T> Size => new( Width, Height );
	
	public AxisAlignedBox2 ( Size2<T> size ) {
		MinX = T.Zero;
		MaxX = size.Width;
		MinY = T.Zero;
		MaxY = size.Height;
	}
	
	public AxisAlignedBox2 ( Point2<T> position, Size2<T> size ) {
		MinX = position.X;
		MaxX = MinX + size.Width;
		MinY = position.Y;
		MaxY = MinY + size.Height;
	}
	
	public AxisAlignedBox2 ( T x, T y, T width, T height ) {
		MinX = x;
		MaxX = MinX + width;
		MinY = y;
		MaxY = MinY + height;
	}
	
	public AxisAlignedBox2<T> Contain ( AxisAlignedBox2<T> other ) {
		return new() {
			MinX = T.Min( MinX, other.MinX ),
			MinY = T.Min( MinY, other.MinY ),
			MaxX = T.Max( MaxX, other.MaxX ),
			MaxY = T.Max( MaxY, other.MaxY )
		};
	}
	
	public static implicit operator AxisAlignedBox2<T> ( Size2<T> size ) => new( size );
	
	public override string ToString () {
		return $"{Size} at {Position}";
	}
}

public static class AABox2<T> where T : INumber<T>, IFloatingPointIeee754<T> {
	public static AxisAlignedBox2<T> Undefined { get; } = new() {
		MinX = T.PositiveInfinity,
		MinY = T.PositiveInfinity,
		MaxX = T.NegativeInfinity,
		MaxY = T.NegativeInfinity
	};
}
