/// This file [AxisAlignedBox3.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.AxisAlignedBoxTemplate and parameter 3 (System.Int32)
using System.Numerics;

namespace Vit.Framework.Mathematics;

public struct AxisAlignedBox3<T> where T : INumber<T> {
	public T MinX;
	public T MaxX;
	public T MinY;
	public T MaxY;
	public T MinZ;
	public T MaxZ;
	
	public T Width => MaxX - MinX;
	public T Height => MaxY - MinY;
	public T Depth => MaxZ - MinZ;
	
	public Point3<T> Position => new( MinX, MinY, MinZ );
	public Size3<T> Size => new( Width, Height, Depth );
	
	public AxisAlignedBox3 ( Size3<T> size ) {
		MinX = T.Zero;
		MaxX = size.Width;
		MinY = T.Zero;
		MaxY = size.Height;
		MinZ = T.Zero;
		MaxZ = size.Depth;
	}
	
	public AxisAlignedBox3 ( Point3<T> position, Size3<T> size ) {
		MinX = position.X;
		MaxX = MinX + size.Width;
		MinY = position.Y;
		MaxY = MinY + size.Height;
		MinZ = position.Z;
		MaxZ = MinZ + size.Depth;
	}
	
	public AxisAlignedBox3 ( T x, T y, T z, T width, T height, T depth ) {
		MinX = x;
		MaxX = MinX + width;
		MinY = y;
		MaxY = MinY + height;
		MinZ = z;
		MaxZ = MinZ + depth;
	}
	
	public AxisAlignedBox3<T> Contain ( AxisAlignedBox3<T> other ) {
		return new() {
			MinX = T.Min( MinX, other.MinX ),
			MinY = T.Min( MinY, other.MinY ),
			MinZ = T.Min( MinZ, other.MinZ ),
			MaxX = T.Max( MaxX, other.MaxX ),
			MaxY = T.Max( MaxY, other.MaxY ),
			MaxZ = T.Max( MaxZ, other.MaxZ )
		};
	}
	
	public static implicit operator AxisAlignedBox3<T> ( Size3<T> size ) => new( size );
	
	public override string ToString () {
		return $"{Size} at {Position}";
	}
}

public static class AABox3<T> where T : INumber<T>, IFloatingPointIeee754<T> {
	public static AxisAlignedBox3<T> Undefined { get; } = new() {
		MinX = T.PositiveInfinity,
		MinY = T.PositiveInfinity,
		MinZ = T.PositiveInfinity,
		MaxX = T.NegativeInfinity,
		MaxY = T.NegativeInfinity,
		MaxZ = T.NegativeInfinity
	};
}
