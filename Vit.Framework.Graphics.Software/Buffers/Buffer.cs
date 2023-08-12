using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Buffers;

namespace Vit.Framework.Graphics.Software.Buffers;

public class Buffer<T> : IHostBuffer<T>, IDeviceBuffer<T>, IByteBuffer where T : unmanaged {
	public byte[] Data { get; private set; } = Array.Empty<byte>();

	public void UploadRaw ( ReadOnlySpan<byte> data, uint offset = 0 ) {
		data.CopyTo( Data.AsSpan().Slice( (int)offset, data.Length ) );
	}
	public void Upload ( ReadOnlySpan<T> data, uint offset = 0 ) {
		UploadRaw( MemoryMarshal.AsBytes( data ), offset * IBuffer<T>.Stride );
	}

	public void AllocateRaw ( uint size, BufferUsage usageHint ) {
		Data = new byte[size];
	}
	public void Allocate ( uint size, BufferUsage usageHint ) {
		AllocateRaw( size * IBuffer<T>.Stride, usageHint );
	}

	public void Dispose () {
		
	}

	public Span<byte> Bytes => Data.AsSpan();
}

public interface IByteBuffer {
	Span<byte> Bytes { get; }
}