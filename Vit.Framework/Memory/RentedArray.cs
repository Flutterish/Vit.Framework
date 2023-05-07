using System.Buffers;

namespace Vit.Framework.Memory;

public struct RentedArray<T> : IDisposable {
	/// <summary>
	/// The size of the array. The underlying rented memory might be larger.
	/// </summary>
	public int Length;
	/// <summary>
	/// The amount of items this rented array can store in the underlying, rented memory.
	/// </summary>
	public int Capacity => array.Length;
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

	/// <summary>
	/// Reallocates this rented array to have the given size.
	/// </summary>
	/// <remarks>
	/// This might simply change the length if it is below the capacity of the rented array or rent new memory.
	/// </remarks>
	public void Reallocate ( int size ) {
		if ( size <= Capacity ) {
			Length = size;
			return;
		}

		var @new = source.Rent( size );
		var copyLength = int.Min( size, Length );
		AsSpan( 0, copyLength ).CopyTo( @new.AsSpan() );
		source.Return( array );
		array = @new;
	}

	/// <summary>
	/// Reallocates this rented array to have the given size. 
	/// If it is reallocated, items from the old one will not be copied over.
	/// </summary>
	/// <remarks>
	/// This might simply change the length if it is below the capacity of the rented array or rent new memory.
	/// </remarks>
	public void ReallocateStorage ( int size ) {
		if ( size <= array.Length ) {
			Length = size;
			return;
		}

		source.Return( array );
		array = source.Rent( size );
	}

	public ref T this[Index index] => ref array[index];
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
