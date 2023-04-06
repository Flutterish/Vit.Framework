using System.Buffers;

namespace Vit.Framework.Allocation;

public struct RentedArray<T> : IDisposable {
	public readonly int Length;
	ArrayPool<T> source;
	T[] array;

	public RentedArray ( int size ) : this( size, ArrayPool<T>.Shared ) { }
	public RentedArray ( int size, ArrayPool<T> pool ) {
		source = pool;
		Length = size;
		array = source.Rent( size );
	}
	public RentedArray ( uint size ) : this( (int)size, ArrayPool<T>.Shared ) { }
	public RentedArray ( uint size, ArrayPool<T> pool ) : this( (int)size, pool ) { }

	public RentedArray ( IReadOnlyList<T> source ) : this( source, ArrayPool<T>.Shared ) { }
	public RentedArray ( IReadOnlyList<T> source, ArrayPool<T> pool ) {
		this.source = pool;
		Length = source.Count;
		array = this.source.Rent( Length );
		for ( int i = 0; i < source.Count; i++ )
			array[i] = source[i];
	}

	public RentedArray ( ReadOnlySpan<T> source ) : this( source, ArrayPool<T>.Shared ) { }
	public RentedArray ( ReadOnlySpan<T> source, ArrayPool<T> pool ) {
		this.source = pool;
		Length = source.Length;
		array = this.source.Rent( Length );
		source.CopyTo( array );
	}

	public void Clear () => AsSpan().Clear();

	public ref T this[int index] => ref array[index];
	public ref T this[nint index] => ref array[index];
	public ref T this[uint index] => ref array[index];
	public ref T this[nuint index] => ref array[index];
	public Span<T> AsSpan () => array.AsSpan( 0, Length );
	public Span<T> AsSpan ( int start ) => array.AsSpan( start, Length - start );
	public Span<T> AsSpan ( int start, int length ) => array.AsSpan( start, length );
	public Span<T>.Enumerator GetEnumerator () => AsSpan().GetEnumerator();

	public static implicit operator Span<T> ( RentedArray<T> array )
		=> array.AsSpan();

	public static implicit operator ReadOnlySpan<T> ( RentedArray<T> array )
		=> array.AsSpan();

	public void Dispose () {
		source.Return( array );
	}
}
