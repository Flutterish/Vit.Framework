/// This file [AxisAlignedBox4.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.AxisAlignedBoxTemplate and parameter 4 (System.Int32)
using System.Numerics;

namespace Vit.Framework.Mathematics;

public struct AxisAlignedBox4<T> where T : INumber<T> {
	public T MinX;
	public T MaxX;
	public T MinY;
	public T MaxY;
	public T MinZ;
	public T MaxZ;
	public T MinW;
	public T MaxW;
	
	public T Width => MaxX - MinX;
	public T Height => MaxY - MinY;
	public T Depth => MaxZ - MinZ;
	public T Anakata => MaxW - MinW;
	
	public Point4<T> Position => new( MinX, MinY, MinZ, MinW );
	public Size4<T> Size => new( Width, Height, Depth, Anakata );
	
	public AxisAlignedBox4 ( Size4<T> size ) {
		MinX = T.Zero;
		MaxX = size.Width;
		MinY = T.Zero;
		MaxY = size.Height;
		MinZ = T.Zero;
		MaxZ = size.Depth;
		MinW = T.Zero;
		MaxW = size.Anakata;
	}
	
	public AxisAlignedBox4 ( Point4<T> position, Size4<T> size ) {
		MinX = position.X;
		MaxX = MinX + size.Width;
		MinY = position.Y;
		MaxY = MinY + size.Height;
		MinZ = position.Z;
		MaxZ = MinZ + size.Depth;
		MinW = position.W;
		MaxW = MinW + size.Anakata;
	}
	
	public AxisAlignedBox4 ( T x, T y, T z, T w, T width, T height, T depth, T anakata ) {
		MinX = x;
		MaxX = MinX + width;
		MinY = y;
		MaxY = MinY + height;
		MinZ = z;
		MaxZ = MinZ + depth;
		MinW = w;
		MaxW = MinW + anakata;
	}
	
	public AxisAlignedBox4<T> Contain ( AxisAlignedBox4<T> other ) {
		return new() {
			MinX = T.Min( MinX, other.MinX ),
			MinY = T.Min( MinY, other.MinY ),
			MinZ = T.Min( MinZ, other.MinZ ),
			MinW = T.Min( MinW, other.MinW ),
			MaxX = T.Max( MaxX, other.MaxX ),
			MaxY = T.Max( MaxY, other.MaxY ),
			MaxZ = T.Max( MaxZ, other.MaxZ ),
			MaxW = T.Max( MaxW, other.MaxW )
		};
	}
	
	public AxisAlignedBox4<T> Intersect ( AxisAlignedBox4<T> other ) {
		return new() {
			MinX = T.Max( MinX, other.MinX ),
			MinY = T.Max( MinY, other.MinY ),
			MinZ = T.Max( MinZ, other.MinZ ),
			MinW = T.Max( MinW, other.MinW ),
			MaxX = T.Min( MaxX, other.MaxX ),
			MaxY = T.Min( MaxY, other.MaxY ),
			MaxZ = T.Min( MaxZ, other.MaxZ ),
			MaxW = T.Min( MaxW, other.MaxW )
		};
	}
	
	public bool Contains ( Point4<T> point )
		=> MinX <= point.X && MaxX >= point.X
		&& MinY <= point.Y && MaxY >= point.Y
		&& MinZ <= point.Z && MaxZ >= point.Z
		&& MinW <= point.W && MaxW >= point.W;
	
	public bool IntersectsWith ( AxisAlignedBox4<T> other ) {
		var intersect = Intersect( other );
		return intersect.Width >= T.Zero
			&& intersect.Height >= T.Zero
			&& intersect.Depth >= T.Zero
			&& intersect.Anakata >= T.Zero;
	}
	
	public static implicit operator AxisAlignedBox4<T> ( Size4<T> size ) => new( size );
	
	public static AxisAlignedBox4<T> operator + ( AxisAlignedBox4<T> left, Vector4<T> right ) => new() {
		MinX = left.MinX + right.X,
		MinY = left.MinY + right.Y,
		MinZ = left.MinZ + right.Z,
		MinW = left.MinW + right.W,
		MaxX = left.MaxX + right.X,
		MaxY = left.MaxY + right.Y,
		MaxZ = left.MaxZ + right.Z,
		MaxW = left.MaxW + right.W
	};
	
	public static AxisAlignedBox4<T> operator - ( AxisAlignedBox4<T> left, Vector4<T> right ) => new() {
		MinX = left.MinX - right.X,
		MinY = left.MinY - right.Y,
		MinZ = left.MinZ - right.Z,
		MinW = left.MinW - right.W,
		MaxX = left.MaxX - right.X,
		MaxY = left.MaxY - right.Y,
		MaxZ = left.MaxZ - right.Z,
		MaxW = left.MaxW - right.W
	};
	
	public override string ToString () {
		return $"{Size} at {Position}";
	}
}

public static class AABox4<T> where T : INumber<T>, IFloatingPointIeee754<T> {
	public static AxisAlignedBox4<T> Undefined { get; } = new() {
		MinX = T.PositiveInfinity,
		MinY = T.PositiveInfinity,
		MinZ = T.PositiveInfinity,
		MinW = T.PositiveInfinity,
		MaxX = T.NegativeInfinity,
		MaxY = T.NegativeInfinity,
		MaxZ = T.NegativeInfinity,
		MaxW = T.NegativeInfinity
	};
}
