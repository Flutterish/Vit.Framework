using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Buffers;

public interface IGlBuffer : IGlObject {
	int Stride { get; }
}

public unsafe class Buffer<T> : DisposableObject, IGlBuffer, IDeviceBuffer<T>, IHostBuffer<T> where T : unmanaged {
	public static readonly int Stride = Marshal.SizeOf( default(T) ); // TODO we still need to split this into host and device optimised buffers
	int IGlBuffer.Stride => Stride;

	public int Handle { get; }
	public readonly BufferTarget Type;
	public T* Data;
	public Buffer ( BufferTarget type ) {
		Type = type;
		Handle = GL.GenBuffer();
	}

	public void Allocate ( uint size, BufferUsage usageHint ) {
		GL.BindBuffer( Type, Handle );

		var length = (int)size * Stride;
		GL.BufferStorage( Type, length, (nint)null, BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit );
		Data = (T*)GL.MapBufferRange( Type, 0, length, BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit );
	}

	public unsafe void Upload ( ReadOnlySpan<T> data, uint offset = 0 ) {
		data.CopyTo( new Span<T>( (void*)this.Data, data.Length + (int)offset ).Slice( (int)offset ) );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteBuffer( Handle );
	}
}
