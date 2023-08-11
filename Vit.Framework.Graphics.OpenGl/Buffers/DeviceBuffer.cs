using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Buffers;

public class DeviceBuffer<T> : DisposableObject, IGlBuffer, IDeviceBuffer<T> where T : unmanaged {
	public static readonly int Stride = Marshal.SizeOf( default( T ) );
	int IGlBuffer.Stride => Stride;

	public int Handle { get; }
	public readonly BufferTarget Type;
	HostBuffer<T> stagingBuffer;
	public DeviceBuffer ( BufferTarget type ) {
		Type = type;
		Handle = GL.GenBuffer();
		stagingBuffer = new( BufferTarget.CopyReadBuffer );
	}

	public void Allocate ( uint size, BufferUsage usageHint ) {
		var length = (int)size * Stride;
		GL.BindBuffer( Type, Handle );
		GL.BufferStorage( Type, length, (nint)null, BufferStorageFlags.None );

		stagingBuffer.Allocate( size, usageHint );
	}

	public unsafe void Upload ( ReadOnlySpan<T> data, uint offset = 0 ) {
		stagingBuffer.Upload( data, offset );
		GL.CopyNamedBufferSubData( stagingBuffer.Handle, Handle, (int)offset, (int)offset, data.Length * Stride );
	}

	protected override void Dispose ( bool disposing ) {
		stagingBuffer.Dispose();
		GL.DeleteBuffer( Handle );
	}
}
