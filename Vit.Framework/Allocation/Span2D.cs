﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vit.Framework.Allocation;

public readonly ref struct Span2D<T> {
	public readonly Span<T> Flat;
	public readonly int Width;
	public readonly int Height;

	public Span2D ( Span<T> flat, int width, int height ) {
		Flat = flat;
		Width = width;
		Height = height;
	}
	public Span2D ( T[,] values ) {
		Width = values.GetLength( 1 );
		Height = values.GetLength( 0 );

		Flat = MemoryMarshal.CreateSpan( ref Unsafe.As<byte, T>( ref MemoryMarshal.GetArrayDataReference( values ) ), values.Length );
	}

	public ref T this[int x, int y] => ref Flat[y * Width + x];

	public Span<T> GetRow ( int y ) => Flat.Slice( y * Width, Width );
	public SpanView<T> GetColumn ( int x ) => new( Flat[x..], Width );

	public static implicit operator ReadOnlySpan2D<T> ( Span2D<T> span )
		=> new( span.Flat, span.Width, span.Height );

	public static implicit operator Span2D<T> ( T[,] data )
		=> new( data );
}
