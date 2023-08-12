using System.Runtime.InteropServices;

namespace Vit.Framework.Graphics.Rendering.Buffers;

/// <summary>
/// GPU-side storage of arbitrary data. This storage might be in gpu-local or cpu-local memory, depending on usage.
/// </summary>
public interface IBuffer : IDisposable {
	Type StoredType { get; }
}

/// <inheritdoc cref="IBuffer"/>
public interface IBuffer<T> : IBuffer where T : unmanaged {
	/// <summary>
	/// Stride of one element.
	/// </summary>
	public static readonly uint Stride = (uint)Marshal.SizeOf( default(T) );
	/// <summary>
	/// Stride of one element when in a uniform buffer - aligned to 256 bytes boundaries.
	/// </summary>
	public static readonly uint UniformBufferStride = (Stride + 255) / 256 * 256;

	Type IBuffer.StoredType => typeof(T);

	/// <summary>
	/// Allocates (clearing any previous data) a new chunk of memory for this buffer.
	/// </summary>
	/// <param name="size">Amount of elements (*not* bytes) that this buffer needs to be able to hold. Must be greater than 0.</param>
	/// <param name="usageHint">Usage hint for the backend to optimize how this buffer is stored.</param>
	void Allocate ( uint size, BufferUsage usageHint ); // TODO add reallocate
}

/// <summary>
/// GPU-side storage of arbitrary data stored in cpu-local memory.
/// </summary>
/// <remarks>
/// This type of buffer should be used for rapidly updated data, such as uniforms, or anything you need to be able to read per-frame.
/// </remarks>
public interface IHostBuffer : IBuffer {

}

/// <inheritdoc cref="IHostBuffer"/>
public interface IHostBuffer<T> : IHostBuffer, IBuffer<T> where T : unmanaged {

	/// <summary>
	/// Uploads data to a buffer.
	/// </summary>
	/// <param name="data">The data to upload.</param>
	/// <param name="offset">Offset (in amount of elements) into the buffer.</param>
	void Upload ( ReadOnlySpan<T> data, uint offset = 0 );

	public void Upload ( T data, uint offset = 0 ) {
		Upload( MemoryMarshal.CreateReadOnlySpan( ref data, 1 ), offset );
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