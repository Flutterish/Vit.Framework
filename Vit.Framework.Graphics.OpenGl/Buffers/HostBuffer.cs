using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Buffers;

public unsafe class HostBuffer<T> : DisposableObject, IGlBuffer, IHostBuffer<T> where T : unmanaged {
	public uint Stride => IBuffer<T>.Stride;

	public int Handle { get; }
	public readonly BufferTarget Type;
	public void* Data;
	public HostBuffer ( BufferTarget type ) {
		Type = type;
		Handle = GL.GenBuffer();
	}

	public void AllocateRaw ( uint size, BufferUsage usageHint ) {
		GL.BindBuffer( Type, Handle );

		GL.BufferStorage( Type, (int)size, (nint)null, BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit | BufferStorageFlags.ClientStorageBit );
		Data = (void*)GL.MapBufferRange( Type, 0, (int)size, BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit );
	}

	public void UploadRaw ( ReadOnlySpan<byte> data, uint offset = 0 ) {
		data.CopyTo( new Span<byte>( (byte*)this.Data + offset, data.Length ) );
	}

	public unsafe void UploadSparseRaw ( ReadOnlySpan<byte> data, uint _size, uint stride, uint offset = 0 ) {
		byte* ptr = (byte*)this.Data + offset;
		var size = (int)_size;
		for ( int i = 0; i < data.Length; i += size ) {
			data.Slice( i, size ).CopyTo( new Span<byte>( ptr, size ) );
			ptr += stride;
		}
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteBuffer( Handle );
	}
}
