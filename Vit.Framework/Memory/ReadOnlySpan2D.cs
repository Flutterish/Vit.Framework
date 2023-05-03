﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vit.Framework.Memory;

public readonly ref struct ReadOnlySpan2D<T> {
	public readonly ReadOnlySpan<T> Flat;
	public readonly int Width;
	public readonly int Height;

	public ReadOnlySpan2D ( ReadOnlySpan<T> flat, int width, int height ) {
		Flat = flat;
		Width = width;
		Height = height;
	}

	public unsafe ReadOnlySpan2D ( T[,] values ) {
		Width = values.GetLength( 1 );
		Height = values.GetLength( 0 );

		Flat = MemoryMarshal.CreateSpan( ref Unsafe.As<byte, T>( ref MemoryMarshal.GetArrayDataReference( values ) ), values.Length );
	}

	public T this[int x, int y] => Flat[y * Width + x];

	public ReadOnlySpan<T> GetRow ( int y ) => Flat.Slice( y * Width, Width );
	public ReadOnlySpanView<T> GetColumn ( int x ) => new( Flat[x..], Width );

	public static implicit operator ReadOnlySpan2D<T> ( T[,] data )
		=> new( data );
}