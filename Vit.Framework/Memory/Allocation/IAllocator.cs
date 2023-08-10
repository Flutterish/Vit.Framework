using Vit.Framework.Interop;

namespace Vit.Framework.Memory.Allocation;

/// <summary>
/// Performs manual memory management operations, usually backed by an unmanaged memory block.
/// </summary>
public unsafe interface IAllocator {
	/// <summary>
	/// Allocates exactly <c><paramref name="size"/></c> bytes. The memory is not guaranteed to be zeroed-out.
	/// </summary>
	/// <remarks>
	/// Might return <see langword="null"/> if there is not enough memory.
	/// </remarks>
	void* Allocate ( nuint size );
	/// <summary>
	/// Attempts to expand or shrink allocated memory in place to <c><paramref name="newSize"/></c> bytes. If growing, the new memory is not guaranteed to be zeroed-out.
	/// </summary>
	/// <param name="moved"><see langword="true"/> if the data was moved to a new place, <see langword="false"/> if the operation was in place.</param>
	/// <remarks>
	/// Might return <see langword="null"/> if there is not enough memory.
	/// </remarks>
	void* Reallocate ( void* ptr, nuint oldSize, nuint newSize, out bool moved );
	/// <summary>
	/// Un-allocates memory, making it available to be allocated again.
	/// </summary>
	void Free ( void* ptr, nuint size );

	/// <summary>
	/// Allocates memory for one instance of <typeparamref name="T"/>. The memory is not guaranteed to be zeroed-out.
	/// </summary>
	/// <remarks>
	/// Might return <see langword="null"/> if there is not enough memory.
	/// </remarks>
	public T* Allocate<T> () where T : unmanaged {
		return (T*)Allocate( SizeOfHelper<T>.Size );
	}
	/// <inheritdoc cref="Free(void*, uint)"/>
	public void Free<T> ( T* ptr ) where T : unmanaged {
		Free( ptr, SizeOfHelper<T>.Size );
	}

	/// <summary>
	/// Allocates memory for <c><paramref name="count"/></c> instances of <typeparamref name="T"/>. The memory is not guaranteed to be zeroed-out.
	/// </summary>
	/// <remarks>
	/// Might return <see langword="null"/> if there is not enough memory.
	/// </remarks>
	public T* Allocate<T> ( int count ) where T : unmanaged {
		return (T*)Allocate( SizeOfHelper<T>.Size * (nuint)count );
	}
	/// <summary>
	/// Attempts to expand or shrink allocated memory in place to <c><paramref name="newCount"/></c> items. If growing, the new memory is not guaranteed to be zeroed-out.
	/// </summary>
	/// <param name="moved"><see langword="true"/> if the data was moved to a new place, <see langword="false"/> if the operation was in place.</param>
	/// <remarks>
	/// Might return <see langword="null"/> if there is not enough memory.
	/// </remarks>
	public T* Reallocate<T> ( T* ptr, int oldCount, int newCount, out bool moved ) where T : unmanaged {
		return (T*)Reallocate( ptr, SizeOfHelper<T>.Size * (nuint)oldCount, SizeOfHelper<T>.Size * (nuint)newCount, out moved );
	}
	/// <inheritdoc cref="Free(void*, nuint)"/>
	public void Free<T> ( T* ptr, int count ) where T : unmanaged {
		Free( ptr, SizeOfHelper<T>.Size * (nuint)count );
	}
}
