using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Interop;

namespace Vit.Framework.Graphics.Software.Buffers;

public class Buffer<T> : IHostBuffer<T>, IDeviceBuffer<T>, IByteBuffer where T : unmanaged {
	public byte[] Data { get; private set; } = Array.Empty<byte>();
	public Span<byte> Bytes => Data.AsSpan();

	public void AllocateRaw ( uint size, BufferUsage usageHint ) {
		Data = new byte[size];
	}

	public void UploadRaw ( ReadOnlySpan<byte> data, uint offset = 0 ) {
		data.CopyTo( Data.AsSpan().Slice( (int)offset, data.Length ) );
	}

	public unsafe void UploadSparseRaw ( ReadOnlySpan<byte> data, uint _size, uint stride, uint offset = 0 ) {
		var ptr = Data.Data() + offset;
		var size = (int)_size;
		for ( int i = 0; i < data.Length; i += size ) {
			data.Slice( i, size ).CopyTo( new Span<byte>( ptr, size ) );
			ptr += stride;
		}
	}

	public void Dispose () {
		
	}
}

public interface IByteBuffer {
	Span<byte> Bytes { get; }
}