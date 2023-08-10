using Vit.Framework.Interop;

namespace Vit.Framework.Memory.Allocation;

/// <summary>
/// Performs manual memory management operations, usually backed by an unmanaged memory block.
/// </summary>
public unsafe interface IAllocator {
	/// <summary>
	/// Allocates at least <c><paramref name="size"/></c> bytes. The memory is not guaranteed to be zeroed-out.
	/// </summary>
	/// <remarks>
	/// Might return <see langword="null"/> if there is not enough memory.
	/// </remarks>
	Allocation Allocate ( nuint size );
	/// <summary>
	/// Attempts to expand or shrink allocated memory in place to <c><paramref name="newSize"/></c> bytes. If growing, the new memory is not guaranteed to be zeroed-out.
	/// </summary>
	/// <param name="moved"><see langword="true"/> if the data was moved to a new place, <see langword="false"/> if the operation was in place.</param>
	/// <remarks>
	/// Might return <see langword="null"/> if there is not enough memory.
	/// </remarks>
	Allocation Reallocate ( void* ptr, nuint newSize, out bool moved );
	/// <summary>
	/// Un-allocates memory, making it available to be allocated again.
	/// </summary>
	void Free ( void* ptr );

	/// <summary>
	/// Allocates memory for <c><paramref name="count"/></c> instances of <typeparamref name="T"/>. The memory is not guaranteed to be zeroed-out.
	/// </summary>
	/// <remarks>
	/// Might return <see langword="null"/> if there is not enough memory.
	/// </remarks>
	public Allocation<T> Allocate<T> ( int count = 1 ) where T : unmanaged {
		return Allocate( SizeOfHelper<T>.Size * (nuint)count ).ToTyped<T>();
	}
	/// <summary>
	/// Attempts to expand or shrink allocated memory in place to <c><paramref name="newCount"/></c> items. If growing, the new memory is not guaranteed to be zeroed-out.
	/// </summary>
	/// <param name="moved"><see langword="true"/> if the data was moved to a new place, <see langword="false"/> if the operation was in place.</param>
	/// <remarks>
	/// Might return <see langword="null"/> if there is not enough memory.
	/// </remarks>
	public Allocation<T> Reallocate<T> ( T* ptr, int newCount, out bool moved ) where T : unmanaged {
		return Reallocate( ptr, SizeOfHelper<T>.Size * (nuint)newCount, out moved ).ToTyped<T>();
	}
	/// <inheritdoc cref="Free(void*)"/>
	public void Free<T> ( T* ptr ) where T : unmanaged {
		Free( (void*)ptr );
	}
}

public unsafe struct Allocation {
	public void* Pointer;
	public nuint Bytes;

	public Allocation ( void* pointer, nuint bytes ) {
		Pointer = pointer;
		Bytes = bytes;
	}

	public void Deconstruct ( out void* ptr, out nuint bytes ) {
		ptr = Pointer;
		bytes = Bytes;
	}

	public static implicit operator void* ( Allocation allocation )
		=> allocation.Pointer;

	public Allocation<T> ToTyped<T> () where T : unmanaged {
		return new() { Bytes = Bytes, Pointer = (T*)Pointer };
	}
}

public unsafe struct Allocation<T> where T : unmanaged {
	public T* Pointer;
	public nuint Bytes;

	public Allocation ( T* pointer, nuint bytes ) {
		Pointer = pointer;
		Bytes = bytes;
	}

	public static implicit operator T* ( Allocation<T> allocation )
		=> allocation.Pointer;

	public void Deconstruct ( out T* ptr, out nuint bytes ) {
		ptr = Pointer;
		bytes = Bytes;
	}
}