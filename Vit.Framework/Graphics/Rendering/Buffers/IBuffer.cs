using System.Runtime.InteropServices;

namespace Vit.Framework.Graphics.Rendering.Buffers;

/// <summary>
/// GPU-side storage of arbitrary data. This storage might be in gpu-local or cpu-local memory, depending on usage.
/// </summary>
public interface IBuffer : IDisposable {
	/// <summary>
	/// Allocates (clearing any previous data) a new chunk of memory for this buffer.
	/// </summary>
	/// <param name="size">Amount of <b>bytes</b> that this buffer needs to be able to hold. Must be greater than 0.</param>
	/// <param name="usageHint">Usage hint for the backend to optimize how this buffer is stored.</param>
	void AllocateRaw ( uint size, BufferUsage usageHint ); // TODO do we really need this? We can just set the size when creating the buffer
}

/// <inheritdoc cref="IBuffer"/>
public interface IBuffer<T> : IBuffer where T : unmanaged {
	/// <summary>
	/// Stride of one element.
	/// </summary>
	public static readonly uint Stride = (uint)Marshal.SizeOf( default(T) );
	public static uint AlignedStride ( uint alinment ) => (Stride + alinment - 1) / alinment * alinment;
}

/// <summary>
/// GPU-side storage of arbitrary data stored in cpu-local memory. Can be temporarily mapped.
/// </summary>
/// <remarks>
/// This type of buffer should be used for rapidly updated data, such as uniforms, or anything you need to be able to read per-frame.
/// </remarks>
public unsafe interface IHostBuffer : IBuffer {
	/// <summary>
	/// Temporarily maps the buffer. This may block GPU access.
	/// </summary>
	void* Map ();
	/// <summary>
	/// Unmaps the buffer, restoring GPU access if it was blocked.
	/// </summary>
	void Unmap ();

	/// <summary>
	/// Uploads data to the buffer.
	/// </summary>
	/// <param name="data">The data to upload.</param>
	/// <param name="offset">Offset (in <b>bytes</b>) into the buffer.</param>
	void UploadRaw ( ReadOnlySpan<byte> data, uint offset = 0 );

	/// <summary>
	/// Uploads data to the buffer, such that <c><paramref name="data"/></c> is chunked into <c><paramref name="size"/></c>-byte chunks and each chunk is placed every <c><paramref name="destinationStride"/></c> bytes in the buffer.
	/// </summary>
	/// <param name="data">The data to upload.</param>
	/// <param name="size">Size of each element in <paramref name="data"/>. Must be smaller or equal to <paramref name="destinationStride"/>.</param>
	/// <param name="sourceStride">Stride of each element in the data. For packed data, equal to <paramref name="size"/>.</param>
	/// <param name="destinationStride">Stride of each element in the buffer.</param>
	/// <param name="offset">Offset (in <b>bytes</b>) into the buffer.</param>
	void UploadSparseRaw ( ReadOnlySpan<byte> data, uint size, uint sourceStride, uint destinationStride, uint offset = 0 );
}

/// <inheritdoc cref="IHostBuffer"/>
public interface IHostBuffer<T> : IHostBuffer, IBuffer<T> where T : unmanaged {
	
}

/// <summary>
/// GPU-side storage of arbitrary data stored in gpu-local memory.
/// </summary>
/// <remarks>
/// This type of buffer should be used for anything the gpu needs rapid access to, and is rarely updated by the cpu, such as mesh data.
/// </remarks>
public interface IDeviceBuffer : IBuffer {

}

/// <inheritdoc cref="IDeviceBuffer"/>
public interface IDeviceBuffer<T> : IDeviceBuffer, IBuffer<T> where T : unmanaged {

}

/// <summary>
/// GPU-side storage of arbitrary data stored in cpu-local memory. Can be persistently mapped.
/// </summary>
/// <remarks>
/// This type of buffer can not be used for drawing, but it can be copied over to other buffers (such as <see cref="IDeviceBuffer"/>).
/// </remarks>
public unsafe interface IStagingBuffer : IBuffer {
	void* GetData ();

	/// <summary>
	/// Copies data from this buffer to another.
	/// </summary>
	/// <param name="buffer">The destination buffer.</param>
	/// <param name="length">Amount of bytes to copy.</param>
	/// <param name="sourceOffset">Offset into this buffer in bytes.</param>
	/// <param name="destinationOffset">Offset into the destinaton buffer in bytes.</param>
	void CopyToRaw ( IHostBuffer buffer, uint length, uint sourceOffset = 0, uint destinationOffset = 0 );

	/// <summary>
	/// Copies data from this buffer to another, such that the data is chunked into <c><paramref name="size"/></c>-byte chunks and each chunk is placed every <c><paramref name="stride"/></c> bytes in destination the buffer.
	/// </summary>
	/// <param name="buffer">The destination buffer.</param>
	/// <param name="length">Amount of bytes to copy.</param>
	/// <param name="size">Amount of bytes per chunk.</param>
	/// <param name="sourceStride">Stride of each element in the data. For packed data, equal to <paramref name="size"/>.</param>
	/// <param name="destinationStride">Stride of each element in the buffer.</param>
	/// <param name="sourceOffset">Offset into this buffer in bytes.</param>
	/// <param name="destinationOffset">Offset into the destinaton buffer in bytes.</param>
	void SparseCopyToRaw ( IHostBuffer buffer, uint length, uint size, uint sourceStride, uint destinationStride, uint sourceOffset = 0, uint destinationOffset = 0 );
}

/// <inheritdoc cref="IStagingBuffer"/>
public unsafe interface IStagingBuffer<T> : IStagingBuffer, IBuffer<T> where T : unmanaged {
	
}

/// <summary>
/// An interface which implements <see cref="IHostBuffer{T}"/> and <see cref="IStagingBuffer{T}"/> for backends where the 2 are not distinct.
/// </summary>
public unsafe interface IHostStagingBuffer<T> : IHostBuffer<T>, IStagingBuffer<T> where T : unmanaged {
	void* IHostBuffer.Map () {
		return GetData();
	}
	void IHostBuffer.Unmap() { }

	void IHostBuffer.UploadRaw( ReadOnlySpan<byte> data, uint offset ) {
		((IStagingBuffer)this).UploadRaw( data, offset );
	}

	void IHostBuffer.UploadSparseRaw( ReadOnlySpan<byte> data, uint size, uint sourceStride, uint destinationStride, uint offset ) {
		((IStagingBuffer)this).UploadSparseRaw( data, size, sourceStride, destinationStride, offset );
	}

	void IStagingBuffer.CopyToRaw( IHostBuffer buffer, uint length, uint sourceOffset, uint destinationOffset ) {
		this.CopyToRaw( (IStagingBuffer)buffer, length, sourceOffset, destinationOffset );
	}

	void IStagingBuffer.SparseCopyToRaw( IHostBuffer buffer, uint length, uint size, uint sourceStride, uint destinationStride, uint sourceOffset, uint destinationOffset ) {
		this.SparseCopyToRaw( (IStagingBuffer)buffer, length, size, sourceStride, destinationStride, sourceOffset, destinationOffset );
	}
}

public static class BufferExtensions {
	/// <summary>
	/// Allocates (clearing any previous data) a new chunk of memory for this buffer.
	/// </summary>
	/// <param name="size">Amount of elements (*not* bytes) that this buffer needs to be able to hold. Must be greater than 0.</param>
	/// <param name="usageHint">Usage hint for the backend to optimize how this buffer is stored.</param>
	public static void Allocate<T> ( this IBuffer<T> @this, uint size, BufferUsage usageHint ) where T : unmanaged {
		@this.AllocateRaw( size * IBuffer<T>.Stride, usageHint );
	}

	/// <summary>
	/// Allocates (clearing any previous data) a new chunk of memory for this buffer. This chunk will be suited to hold uniform data (aligned to 256 bytes).
	/// </summary>
	/// <param name="size">Amount of elements (*not* bytes) that this buffer needs to be able to hold. Must be greater than 0.</param>
	/// <param name="usageHint">Usage hint for the backend to optimize how this buffer is stored.</param>
	public static void AllocateUniform<T> ( this IBuffer<T> @this, uint size, BufferUsage usageHint ) where T : unmanaged {
		@this.AllocateAligned( size, 256, usageHint );
	}

	/// <summary>
	/// Allocates (clearing any previous data) a new chunk of memory for this buffer. This chunk will pad between elements such that every element will be aligned to the given alignemnt.
	/// </summary>
	/// <param name="size">Amount of elements (*not* bytes) that this buffer needs to be able to hold. Must be greater than 0.</param>
	/// <param name="alignment">Alignment of elements in bytes.</param>
	/// <param name="usageHint">Usage hint for the backend to optimize how this buffer is stored.</param>
	public static void AllocateAligned<T> ( this IBuffer<T> @this, uint size, uint alignment, BufferUsage usageHint ) where T : unmanaged {
		var stride = (IBuffer<T>.Stride + alignment - 1) / alignment * alignment;
		@this.AllocateRaw( (size - 1) * stride + IBuffer<T>.Stride, usageHint );
	}

	/// <summary>
	/// Uploads data to the buffer.
	/// </summary>
	/// <param name="data">The data to upload.</param>
	/// <param name="offset">Offset (in amount of elements) into the buffer.</param>
	public static void Upload<T> ( this IHostBuffer<T> @this, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		@this.UploadRaw( MemoryMarshal.AsBytes( data ), offset * IBuffer<T>.Stride );
	}

	/// <inheritdoc cref="Upload{T}(IHostBuffer{T}, ReadOnlySpan{T}, uint)"/>
	public static void Upload<T> ( this IHostBuffer<T> @this, T data, uint offset = 0 ) where T : unmanaged {
		@this.Upload( MemoryMarshal.CreateReadOnlySpan( ref data, 1 ), offset );
	}

	/// <summary>
	/// Uploads data to the buffer. The uploaded data will be suited to hold uniform data (aligned to 256 bytes).
	/// </summary>
	/// <param name="data">The data to upload.</param>
	/// <param name="offset">Offset (in amount of elements) into the buffer.</param>
	public static void UploadUniform<T> ( this IHostBuffer<T> @this, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		@this.UploadAligned( data, 256, offset );
	}

	/// <inheritdoc cref="UploadUniform{T}(IHostBuffer{T}, ReadOnlySpan{T}, uint)"/>
	public static void UploadUniform<T> ( this IHostBuffer<T> @this, T data, uint offset = 0 ) where T : unmanaged {
		@this.UploadUniform( MemoryMarshal.CreateReadOnlySpan( ref data, 1 ), offset );
	}

	/// <summary>
	/// Uploads data to the buffer. The data wil be uploaded such that each element is placed on an alignment boundary.
	/// </summary>
	/// <param name="data">The data to upload.</param>
	/// <param name="alignment">Alignment of elements in bytes.</param>
	/// <param name="offset">Offset (in amount of elements) into the buffer.</param>
	public static void UploadAligned<T> ( this IHostBuffer<T> @this, ReadOnlySpan<T> data, uint alignment, uint offset = 0 ) where T : unmanaged {
		var stride = IBuffer<T>.AlignedStride( alignment );
		@this.UploadSparseRaw( MemoryMarshal.AsBytes( data ), IBuffer<T>.Stride, IBuffer<T>.Stride, stride, offset * stride );
	}

	/// <inheritdoc cref="UploadAligned{T}(IHostBuffer{T}, ReadOnlySpan{T}, uint, uint)"/>
	public static void UploadAligned<T> ( this IHostBuffer<T> @this, T data, uint alignment, uint offset = 0 ) where T : unmanaged {
		@this.UploadAligned( MemoryMarshal.CreateReadOnlySpan( ref data, 1 ), alignment, offset );
	}

	/// <inheritdoc cref="IStagingBuffer.CopyToRaw(IHostBuffer, uint, uint, uint)"/>
	public static void CopyToRaw ( this IStagingBuffer @this, IStagingBuffer buffer, uint length, uint sourceOffset = 0, uint destinationOffset = 0 ) {
		buffer.UploadRaw( @this.AsSpan<byte>( (int)sourceOffset, (int)length ), destinationOffset );
	}

	/// <inheritdoc cref="IStagingBuffer.SparseCopyToRaw(IHostBuffer, uint, uint, uint, uint, uint, uint)"/>
	public static void SparseCopyToRaw ( this IStagingBuffer @this, IStagingBuffer buffer, uint length, uint size, uint sourceStride, uint destinationStride, uint sourceOffset = 0, uint destinationOffset = 0 ) {
		buffer.UploadSparseRaw( @this.AsSpan<byte>( (int)sourceOffset, (int)length ), size, sourceStride, destinationStride, destinationOffset );
	}

	public static unsafe Span<T> AsSpan<T> ( this IStagingBuffer @this, int length ) where T : unmanaged
		=> new Span<T>( (T*)@this.GetData(), length );

	public static unsafe Span<T> AsSpan<T> ( this IStagingBuffer @this, int start, int length ) where T : unmanaged
		=> new Span<T>( (T*)@this.GetData() + start, length );

	/// <inheritdoc cref="IHostBuffer.UploadRaw(ReadOnlySpan{byte}, uint)"/>
	public static unsafe void UploadRaw ( this IStagingBuffer @this, ReadOnlySpan<byte> data, uint offset = 0 ) {
		data.CopyTo( new Span<byte>( (byte*)@this.GetData() + offset, data.Length ) );
	}

	/// <inheritdoc cref="IHostBuffer.UploadSparseRaw(ReadOnlySpan{byte}, uint, uint, uint, uint)"/>
	public static unsafe void UploadSparseRaw ( this IStagingBuffer @this, ReadOnlySpan<byte> data, uint size, uint sourceStride, uint destinationStride, uint offset = 0 ) {
		var ptr = (byte*)@this.GetData() + offset;
		var Size = (int)size;
		for ( int i = 0; i < data.Length; i += (int)sourceStride ) {
			data.Slice( i, Size ).CopyTo( new Span<byte>( ptr, Size ) );
			ptr += destinationStride;
		}
	}

	public static unsafe Span<T> AsSpan<T> ( this IStagingBuffer<T> @this, int length ) where T : unmanaged
		=> new Span<T>( (T*)@this.GetData(), length );

	public static unsafe Span<T> AsSpan<T> ( this IStagingBuffer<T> @this, int start, int length ) where T : unmanaged
		=> new Span<T>( (T*)@this.GetData() + start, length );

	/// <summary>
	/// Copies data from this buffer to another.
	/// </summary>
	/// <param name="buffer">The destination buffer.</param>
	/// <param name="length">Amount of elements to copy.</param>
	/// <param name="sourceOffset">Offset into this buffer in elements.</param>
	/// <param name="destinationOffset">Offset into the destinaton buffer in elements.</param>
	public static void CopyTo<T> ( this IStagingBuffer<T> @this, IHostBuffer<T> buffer, uint length, uint sourceOffset = 0, uint destinationOffset = 0 ) where T : unmanaged {
		@this.CopyToRaw( buffer, length * IBuffer<T>.Stride, sourceOffset * IBuffer<T>.Stride, destinationOffset * IBuffer<T>.Stride );
	}
	/// <inheritdoc cref="CopyTo{T}(IStagingBuffer{T}, IHostBuffer{T}, uint, uint, uint)"/>
	public static void CopyTo<T> ( this IStagingBuffer<T> @this, IStagingBuffer<T> buffer, uint length, uint sourceOffset = 0, uint destinationOffset = 0 ) where T : unmanaged {
		@this.CopyToRaw( buffer, length * IBuffer<T>.Stride, sourceOffset * IBuffer<T>.Stride, destinationOffset * IBuffer<T>.Stride );
	}

	/// <summary>
	/// Copies data from this buffer to another. The uploaded data will be suited to hold uniform data (aligned to 256 bytes).
	/// Assumes this buffer is packed.
	/// </summary>
	/// <param name="buffer">The destination buffer.</param>
	/// <param name="length">Amount of elements to copy.</param>
	/// <param name="sourceOffset">Offset into this buffer in elements.</param>
	/// <param name="destinationOffset">Offset into the destinaton buffer in elements.</param>
	public static void PackedCopyUniformTo<T> ( this IStagingBuffer<T> @this, IHostBuffer<T> buffer, uint length, uint sourceOffset = 0, uint destinationOffset = 0 ) where T : unmanaged {
		@this.CopyAlignedTo( buffer, length, 256, sourceOffset, destinationOffset );
	}
	/// <inheritdoc cref="PackedCopyUniformTo{T}(IStagingBuffer{T}, IHostBuffer{T}, uint, uint, uint)"/>
	public static void PackedCopyUniformTo<T> ( this IStagingBuffer<T> @this, IStagingBuffer<T> buffer, uint length, uint sourceOffset = 0, uint destinationOffset = 0 ) where T : unmanaged {
		@this.CopyAlignedTo( buffer, length, 256, sourceOffset, destinationOffset );
	}

	/// <summary>
	/// Copies data from this buffer to another. The uploaded data will be suited to hold uniform data (aligned to 256 bytes).
	/// Assumes this buffer's elements are aligned to 256.
	/// </summary>
	/// <param name="buffer">The destination buffer.</param>
	/// <param name="length">Amount of elements to copy.</param>
	/// <param name="sourceOffset">Offset into this buffer in elements.</param>
	/// <param name="destinationOffset">Offset into the destinaton buffer in elements.</param>
	public static void UniformCopyUniformTo<T> ( this IStagingBuffer<T> @this, IHostBuffer<T> buffer, uint length, uint sourceOffset = 0, uint destinationOffset = 0 ) where T : unmanaged {
		var alignedStride = IBuffer<T>.AlignedStride( 256 );
		@this.SparseCopyToRaw( buffer, (length - 1) * alignedStride + IBuffer<T>.Stride, IBuffer<T>.Stride, alignedStride, alignedStride, sourceOffset * alignedStride, destinationOffset * alignedStride );
	}
	/// <inheritdoc cref="UniformCopyUniformTo{T}(IStagingBuffer{T}, IHostBuffer{T}, uint, uint, uint)"/>
	public static void UniformCopyUniformTo<T> ( this IStagingBuffer<T> @this, IStagingBuffer<T> buffer, uint length, uint sourceOffset = 0, uint destinationOffset = 0 ) where T : unmanaged {
		var alignedStride = IBuffer<T>.AlignedStride( 256 );
		@this.SparseCopyToRaw( buffer, (length - 1) * alignedStride + IBuffer<T>.Stride, IBuffer<T>.Stride, alignedStride, alignedStride, sourceOffset * alignedStride, destinationOffset * alignedStride );
	}

	/// <summary>
	/// Copies data from this buffer to another. The data wil be uploaded such that each element is placed on an alignment boundary.
	/// Assumes this buffer is packed.
	/// </summary>
	/// <param name="buffer">The destination buffer.</param>
	/// <param name="length">Amount of elements to copy.</param>
	/// <param name="alignment">Alignment of elements in bytes.</param>
	/// <param name="sourceOffset">Offset into this buffer in elements.</param>
	/// <param name="destinationOffset">Offset into the destinaton buffer in elements.</param>
	public static void CopyAlignedTo<T> ( this IStagingBuffer<T> @this, IHostBuffer<T> buffer, uint length, uint alignment, uint sourceOffset = 0, uint destinationOffset = 0 ) where T : unmanaged {
		@this.SparseCopyToRaw( buffer, length * IBuffer<T>.Stride, length, length, IBuffer<T>.AlignedStride( alignment ), sourceOffset * IBuffer<T>.Stride, destinationOffset * IBuffer<T>.AlignedStride( alignment ) );
	}
	/// <inheritdoc cref="CopyAlignedTo{T}(IStagingBuffer{T}, IHostBuffer{T}, uint, uint, uint, uint)"/>
	public static void CopyAlignedTo<T> ( this IStagingBuffer<T> @this, IStagingBuffer<T> buffer, uint length, uint alignment, uint sourceOffset = 0, uint destinationOffset = 0 ) where T : unmanaged {
		@this.SparseCopyToRaw( buffer, length * IBuffer<T>.Stride, length, length, IBuffer<T>.AlignedStride( alignment ), sourceOffset * IBuffer<T>.Stride, destinationOffset * IBuffer<T>.AlignedStride( alignment ) );
	}

	/// <inheritdoc cref="Upload{T}(IHostBuffer{T}, ReadOnlySpan{T}, uint)"/>
	public static void Upload<T> ( this IStagingBuffer<T> @this, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		@this.UploadRaw( MemoryMarshal.AsBytes( data ), offset * IBuffer<T>.Stride );
	}

	/// <inheritdoc cref="Upload{T}(IHostBuffer{T}, T, uint)"/>
	public static void Upload<T> ( this IStagingBuffer<T> @this, T data, uint offset = 0 ) where T : unmanaged {
		@this.Upload( MemoryMarshal.CreateReadOnlySpan( ref data, 1 ), offset );
	}

	/// <inheritdoc cref="UploadUniform{T}(IHostBuffer{T}, ReadOnlySpan{T}, uint)"/>
	public static void UploadUniform<T> ( this IStagingBuffer<T> @this, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		@this.UploadAligned( data, 256, offset );
	}

	/// <inheritdoc cref="UploadUniform{T}(IHostBuffer{T}, T, uint)"/>
	public static void UploadUniform<T> ( this IStagingBuffer<T> @this, T data, uint offset = 0 ) where T : unmanaged {
		@this.UploadUniform( MemoryMarshal.CreateReadOnlySpan( ref data, 1 ), offset );
	}

	/// <inheritdoc cref="UploadAligned{T}(IHostBuffer{T}, ReadOnlySpan{T}, uint, uint)"/>
	public static void UploadAligned<T> ( this IStagingBuffer<T> @this, ReadOnlySpan<T> data, uint alignment, uint offset = 0 ) where T : unmanaged {
		var stride = IBuffer<T>.AlignedStride( alignment );
		@this.UploadSparseRaw( MemoryMarshal.AsBytes( data ), IBuffer<T>.Stride, IBuffer<T>.Stride, stride, offset * stride );
	}

	/// <inheritdoc cref="UploadAligned{T}(IHostBuffer{T}, T, uint, uint)"/>
	public static void UploadAligned<T> ( this IStagingBuffer<T> @this, T data, uint alignment, uint offset = 0 ) where T : unmanaged {
		@this.UploadAligned( MemoryMarshal.CreateReadOnlySpan( ref data, 1 ), alignment, offset );
	}
}