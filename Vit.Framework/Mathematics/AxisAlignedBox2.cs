﻿using System.Numerics;

namespace Vit.Framework.Mathematics;

public struct AxisAlignedBox2<T> where T : INumber<T> {
	public T MinX;
	public T MinY;
	public T MaxX;
	public T MaxY;

	public T Width => MaxX - MinX;
	public T Height => MaxY - MinY;

	public Point2<T> Position => new( MinX, MinY );
	public Size2<T> Size => new( Width, Height );

	public AxisAlignedBox2 ( Size2<T> size ) {
		MinX = T.Zero;
		MinY = T.Zero;
		MaxX = size.Width;
		MaxY = size.Height;
	}
	public static implicit operator AxisAlignedBox2<T> ( Size2<T> size ) => new( size );

	public AxisAlignedBox2 ( Point2<T> position, Size2<T> size ) {
		MinX = position.X;
		MinY = position.Y;
		MaxX = MinX + size.Width;
		MaxY = MinY + size.Height;
	}

	public AxisAlignedBox2<T> Contain ( AxisAlignedBox2<T> other ) {
		return new AxisAlignedBox2<T>() {
			MinX = T.Min( MinX, other.MinX ),
			MinY = T.Min( MinY, other.MinY ),
			MaxX = T.Max( MaxX, other.MaxX ),
			MaxY = T.Max( MaxY, other.MaxY )
		};
	}
}

public static class AABox<T> where T : INumber<T>, IFloatingPointIeee754<T> {
	public static AxisAlignedBox2<T> Undefined { get; } = new() {
		MinX = T.PositiveInfinity,
		MinY = T.PositiveInfinity,
		MaxX = T.NegativeInfinity,
		MaxY = T.NegativeInfinity
	};
}