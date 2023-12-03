/// This file [AxisAlignedBox2.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.AxisAlignedBoxTemplate and parameter 2 (System.Int32)
using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;

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
	
	public AxisAlignedBox2<T> Intersect ( AxisAlignedBox2<T> other ) {
		return new() {
			MinX = T.Max( MinX, other.MinX ),
			MinY = T.Max( MinY, other.MinY ),
			MaxX = T.Min( MaxX, other.MaxX ),
			MaxY = T.Min( MaxY, other.MaxY )
		};
	}
	
	public bool Contains ( Point2<T> point )
		=> MinX <= point.X && MaxX >= point.X
		&& MinY <= point.Y && MaxY >= point.Y;
	
	public bool IntersectsWith ( AxisAlignedBox2<T> other ) {
		var intersect = Intersect( other );
		return intersect.Width >= T.Zero
			&& intersect.Height >= T.Zero;
	}
	
	public static implicit operator AxisAlignedBox2<T> ( Size2<T> size ) => new( size );
	
	public static AxisAlignedBox2<T> operator + ( AxisAlignedBox2<T> left, Vector2<T> right ) => new() {
		MinX = left.MinX + right.X,
		MinY = left.MinY + right.Y,
		MaxX = left.MaxX + right.X,
		MaxY = left.MaxY + right.Y
	};
	
	public static AxisAlignedBox2<T> operator - ( AxisAlignedBox2<T> left, Vector2<T> right ) => new() {
		MinX = left.MinX - right.X,
		MinY = left.MinY - right.Y,
		MaxX = left.MaxX - right.X,
		MaxY = left.MaxY - right.Y
	};
	
	public static implicit operator Quad2<T> ( AxisAlignedBox2<T> box ) => new() {
		PointA = new( box.MinX, box.MinY ),
		PointB = new( box.MaxX, box.MinY ),
		PointC = new( box.MaxX, box.MaxY ),
		PointD = new( box.MinX, box.MaxY )
	};
	
	public static Quad2<T> operator * ( AxisAlignedBox2<T> box, Matrix3<T> matrix )
		=> ((Quad2<T>)box) * matrix;
	
	public static Quad2<T> operator * ( AxisAlignedBox2<T> box, Matrix2<T> matrix )
		=> ((Quad2<T>)box) * matrix;
	
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
