using System.Runtime.InteropServices;

namespace Vit.Framework.Interop;

public struct ByteArrayView<T> where T : unmanaged {
	public byte[] Bytes;
	public int ByteOffset;
	public int ByteLength;

	public ByteArrayView ( byte[] bytes ) {
		Bytes = bytes;
		ByteLength = bytes.Length;
	}

	public Span<byte> AsByteSpan ()
		=> Bytes.AsSpan( ByteOffset, ByteLength );

	public Span<T> AsSpan ()
		=> MemoryMarshal.Cast<byte, T>( AsByteSpan() );

	public ByteArrayView<T> Slice ( int start ) {
		start *= SizeOfHelper<T>.SignedSize;
		return new() { Bytes = Bytes, ByteOffset = ByteOffset + start, ByteLength = ByteLength - start };
	}

	public ByteArrayView<T> Slice ( int start, int length ) {
		start *= SizeOfHelper<T>.SignedSize;
		length *= SizeOfHelper<T>.SignedSize;
		return new() { Bytes = Bytes, ByteOffset = ByteOffset + start, ByteLength = length };
	}

	public ByteArrayView<TTo> Cast<TTo> () where TTo : unmanaged
		=> new() { Bytes = Bytes, ByteOffset = ByteOffset, ByteLength = ByteLength };

	public ref T this[int i] => ref AsSpan()[i];
	public ref T this[uint i] => ref AsSpan()[i];
}
