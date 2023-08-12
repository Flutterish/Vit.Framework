using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Buffers;

public interface IGlDeviceBuffer : IGlBuffer, IDeviceBuffer {
	void UploadRaw ( ReadOnlySpan<byte> data, uint offset = 0 );
}

public class DeviceBuffer<T> : DisposableObject, IGlDeviceBuffer, IDeviceBuffer<T> where T : unmanaged {
	public uint Stride => Type == BufferTarget.UniformBuffer ? IBuffer<T>.UniformBufferStride : IBuffer<T>.Stride;

	public int Handle { get; }
	public readonly BufferTarget Type;
	HostBuffer<T> stagingBuffer;
	public DeviceBuffer ( BufferTarget type ) {
		Type = type;
		Handle = GL.GenBuffer();
		stagingBuffer = new( BufferTarget.CopyReadBuffer );
	}

	public void AllocateRaw ( uint size, BufferUsage usageHint ) {
		GL.BindBuffer( Type, Handle );
		GL.BufferStorage( Type, (int)size, (nint)null, BufferStorageFlags.None );

		stagingBuffer.AllocateRaw( size, usageHint );
	}

	public void Allocate ( uint size, BufferUsage usageHint ) {
		AllocateRaw( size * Stride, usageHint );
	}

	public void UploadRaw ( ReadOnlySpan<byte> data, uint offset = 0 ) {
		stagingBuffer.UploadRaw( data, offset );
		GL.CopyNamedBufferSubData( stagingBuffer.Handle, Handle, (int)offset, (int)offset, data.Length );
	}

	public unsafe void Upload ( ReadOnlySpan<T> data, uint offset = 0 ) {
		stagingBuffer.Upload( data, offset );
		GL.CopyNamedBufferSubData( stagingBuffer.Handle, Handle, (int)(offset * Stride), (int)(offset * Stride), data.Length * (int)Stride );
	}

	protected override void Dispose ( bool disposing ) {
		stagingBuffer.Dispose();
		GL.DeleteBuffer( Handle );
	}
}
