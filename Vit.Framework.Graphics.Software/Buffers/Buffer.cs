using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Interop;

namespace Vit.Framework.Graphics.Software.Buffers;

public class Buffer<T> : IDeviceBuffer<T>, IHostStagingBuffer<T>, IByteBuffer where T : unmanaged {
	public byte[] Data { get; private set; } = Array.Empty<byte>();
	public Span<byte> Bytes => Data.AsSpan();

	public void AllocateRaw ( uint size, BufferUsage usageHint ) {
		Data = new byte[size];
	}

	public unsafe void* GetData () {
		throw new NotImplementedException();
		//return Data.Data();
	}

	public void Dispose () {

	}
}

public interface IByteBuffer {
	Span<byte> Bytes { get; }
}