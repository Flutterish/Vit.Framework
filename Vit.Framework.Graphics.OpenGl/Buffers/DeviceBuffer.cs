using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Buffers;

public class DeviceBuffer<T> : DisposableObject, IGlBuffer, IDeviceBuffer<T> where T : unmanaged {
	public uint Stride => Type == BufferTarget.UniformBuffer ? IBuffer<T>.UniformBufferStride : IBuffer<T>.Stride;

	public int Handle { get; }
	public readonly BufferTarget Type;
	HostBuffer<T> stagingBuffer;
	public DeviceBuffer ( BufferTarget type ) {
		Type = type;
		Handle = GL.GenBuffer();
		stagingBuffer = new( BufferTarget.CopyReadBuffer );
	}

	public void Allocate ( uint size, BufferUsage usageHint ) {
		var length = size * Stride;
		GL.BindBuffer( Type, Handle );
		GL.BufferStorage( Type, (int)length, (nint)null, BufferStorageFlags.None );

		stagingBuffer.Allocate( size, usageHint );
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
