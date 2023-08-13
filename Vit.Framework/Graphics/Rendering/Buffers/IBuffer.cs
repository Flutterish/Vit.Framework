using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vit.Framework.Graphics.Rendering.Buffers;

/// <summary>
/// GPU-side storage of arbitrary data. This storage might be in gpu-local or cpu-local memory, depending on usage.
/// </summary>
public interface IBuffer : IDisposable {
	Type StoredType { get; }

	/// <summary>
	/// Allocates (clearing any previous data) a new chunk of memory for this buffer.
	/// </summary>
	/// <param name="size">Amount of <b>bytes</b> that this buffer needs to be able to hold. Must be greater than 0.</param>
	/// <param name="usageHint">Usage hint for the backend to optimize how this buffer is stored.</param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	void AllocateRaw ( uint size, BufferUsage usageHint );
}

/// <inheritdoc cref="IBuffer"/>
public interface IBuffer<T> : IBuffer where T : unmanaged {
	/// <summary>
	/// Stride of one element.
	/// </summary>
	public static readonly uint Stride = (uint)Marshal.SizeOf( default(T) );
	public static uint AlignedStride ( uint alinment ) => (Stride + alinment - 1) / alinment * alinment;

	Type IBuffer.StoredType => typeof(T);

	/// <summary>
	/// Allocates (clearing any previous data) a new chunk of memory for this buffer.
	/// </summary>
	/// <param name="size">Amount of elements (*not* bytes) that this buffer needs to be able to hold. Must be greater than 0.</param>
	/// <param name="usageHint">Usage hint for the backend to optimize how this buffer is stored.</param>
	public void Allocate ( uint size, BufferUsage usageHint ) {
		AllocateRaw( size * Stride, usageHint );
	}

	/// <summary>
	/// Allocates (clearing any previous data) a new chunk of memory for this buffer. This chunk will be suited to hold uniform data (aligned to 256 bytes).
	/// </summary>
	/// <param name="size">Amount of elements (*not* bytes) that this buffer needs to be able to hold. Must be greater than 0.</param>
	/// <param name="usageHint">Usage hint for the backend to optimize how this buffer is stored.</param>
	public void AllocateUniform ( uint size, BufferUsage usageHint ) {
		AllocateAligned( size, 256, usageHint );
	}

	/// <summary>
	/// Allocates (clearing any previous data) a new chunk of memory for this buffer. This chunk will pad between elements such that every element will be aligned to the given alignemnt.
	/// </summary>
	/// <param name="size">Amount of elements (*not* bytes) that this buffer needs to be able to hold. Must be greater than 0.</param>
	/// <param name="alignment">Alignment of elements in bytes.</param>
	/// <param name="usageHint">Usage hint for the backend to optimize how this buffer is stored.</param>
	public void AllocateAligned ( uint size, uint alignment, BufferUsage usageHint ) {
		var stride = (Stride + alignment - 1) / alignment * alignment;
		AllocateRaw( (size - 1) * stride + Stride, usageHint );
	}
}

/// <summary>
/// GPU-side storage of arbitrary data stored in cpu-local memory.
/// </summary>
/// <remarks>
/// This type of buffer should be used for rapidly updated data, such as uniforms, or anything you need to be able to read per-frame.
/// </remarks>
public interface IHostBuffer : IBuffer {
	/// <summary>
	/// Uploads data to the buffer.
	/// </summary>
	/// <param name="data">The data to upload.</param>
	/// <param name="offset">Offset (in <b>bytes</b>) into the buffer.</param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	void UploadRaw ( ReadOnlySpan<byte> data, uint offset = 0 );

	/// <summary>
	/// Uploads data to the buffer, such that <c><paramref name="data"/></c> is chunked into <c><paramref name="size"/></c>-byte chunks and each chunk is placed every <c><paramref name="stride"/></c> bytes in the buffer.
	/// </summary>
	/// <param name="data">The data to upload.</param>
	/// <param name="size">Size of each element in <paramref name="data"/>. Must be smaller or equal to <paramref name="stride"/>.</param>
	/// <param name="stride">Stride of each element in the buffer.</param>
	/// <param name="offset">Offset (in <b>bytes</b>) into the buffer.</param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	void UploadSparseRaw ( ReadOnlySpan<byte> data, uint size, uint stride, uint offset = 0 );
}

/// <inheritdoc cref="IHostBuffer"/>
public interface IHostBuffer<T> : IHostBuffer, IBuffer<T> where T : unmanaged {
	/// <summary>
	/// Uploads data to the buffer.
	/// </summary>
	/// <param name="data">The data to upload.</param>
	/// <param name="offset">Offset (in amount of elements) into the buffer.</param>
	public void Upload ( ReadOnlySpan<T> data, uint offset = 0 ) {
		UploadRaw( MemoryMarshal.AsBytes( data ), offset * Stride );
	}

	/// <inheritdoc cref="Upload(ReadOnlySpan{T}, uint)"/>
	public void Upload ( T data, uint offset = 0 ) {
		Upload( MemoryMarshal.CreateReadOnlySpan( ref data, 1 ), offset );
	}

	/// <summary>
	/// Uploads data to the buffer. The uploaded data will be suited to hold uniform data (aligned to 256 bytes).
	/// </summary>
	/// <param name="data">The data to upload.</param>
	/// <param name="offset">Offset (in amount of elements) into the buffer.</param>
	public void UploadUniform ( ReadOnlySpan<T> data, uint offset = 0 ) {
		UploadAligned( data, 256, offset );
	}

	/// <inheritdoc cref="UploadUniform(ReadOnlySpan{T}, uint)"/>
	public void UploadUniform ( T data, uint offset = 0 ) {
		UploadUniform( MemoryMarshal.CreateReadOnlySpan( ref data, 1 ), offset );
	}

	/// <summary>
	/// Uploads data to the buffer. The data wil be uploaded such that each element is placed on an alignment boundary.
	/// </summary>
	/// <param name="data">The data to upload.</param>
	/// <param name="alignment">Alignment of elements in bytes.</param>
	/// <param name="offset">Offset (in amount of elements) into the buffer.</param>
	public void UploadAligned ( ReadOnlySpan<T> data, uint alignment, uint offset = 0 ) {
		var stride = (Stride + alignment - 1) / alignment * alignment;
		UploadSparseRaw( MemoryMarshal.AsBytes( data ), Stride, stride, offset * stride );
	}

	/// <inheritdoc cref="UploadAligned(ReadOnlySpan{T}, uint, uint)"/>
	public void UploadAligned ( T data, uint alignment, uint offset = 0 ) {
		UploadAligned( MemoryMarshal.CreateReadOnlySpan( ref data, 1 ), alignment, offset );
	}
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

	public Span<T> AsSpan<T> ( int length ) where T : unmanaged
		=> new Span<T>( (T*)GetData(), length );

	public Span<T> AsSpan<T> ( int start, int length ) where T : unmanaged 
		=> new Span<T>( (T*)GetData() + start, length );
}

/// <inheritdoc cref="IStagingBuffer"/>
public unsafe interface IStagingBuffer<T> : IStagingBuffer, IBuffer<T> where T : unmanaged {
	public Span<T> AsSpan ( int length )
		=> new Span<T>( (T*)GetData(), length );

	public Span<T> AsSpan ( int start, int length )
		=> new Span<T>( (T*)GetData() + start, length );
}