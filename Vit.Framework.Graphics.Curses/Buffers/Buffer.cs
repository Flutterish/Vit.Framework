using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Buffers;

namespace Vit.Framework.Graphics.Curses.Buffers;

public class Buffer<T> : IHostBuffer<T>, IDeviceBuffer<T>, IByteBuffer where T : unmanaged {
	public T[] Data { get; private set; } = Array.Empty<T>();

	public void Upload ( ReadOnlySpan<T> data, uint offset = 0 ) {
		data.CopyTo( Data.AsSpan().Slice( (int)offset, data.Length ) );
	}

	public void Allocate ( uint size, BufferUsage usageHint ) {
		Data = new T[size];
	}

	public void Dispose () {
		
	}

	public ReadOnlySpan<byte> Bytes => MemoryMarshal.AsBytes( Data == null ? Span<T>.Empty : Data.AsSpan() );
}

public interface IByteBuffer {
	ReadOnlySpan<byte> Bytes { get; }
}