using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Buffers;

public unsafe class HostBuffer<T> : DisposableObject, IGlBuffer, IHostBuffer<T> where T : unmanaged {
	public static readonly int Stride = Marshal.SizeOf( default(T) );
	int IGlBuffer.Stride => Stride;

	public int Handle { get; }
	public readonly BufferTarget Type;
	public T* Data;
	public HostBuffer ( BufferTarget type ) {
		Type = type;
		Handle = GL.GenBuffer();
	}

	public void Allocate ( uint size, BufferUsage usageHint ) {
		GL.BindBuffer( Type, Handle );

		var length = (int)size * Stride;
		GL.BufferStorage( Type, length, (nint)null, BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit | BufferStorageFlags.ClientStorageBit );
		Data = (T*)GL.MapBufferRange( Type, 0, length, BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit );
	}

	public unsafe void Upload ( ReadOnlySpan<T> data, uint offset = 0 ) {
		data.CopyTo( new Span<T>( (void*)this.Data, data.Length + (int)offset ).Slice( (int)offset ) );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteBuffer( Handle );
	}
}
