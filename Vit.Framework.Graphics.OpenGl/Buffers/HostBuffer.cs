using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Buffers;

public unsafe class HostBuffer<T> : DisposableObject, IGlBuffer, IHostStagingBuffer<T> where T : unmanaged {
	public int Handle { get; private set; }
	public void* Data;
	public HostBuffer ( uint size, BufferTarget type, BufferUsage usage ) {
		GL.CreateBuffers( 1, out int handle );

		BufferStorageFlags storageFlags = BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit | BufferStorageFlags.ClientStorageBit;
		BufferAccessMask accessMask = BufferAccessMask.MapCoherentBit | BufferAccessMask.MapPersistentBit;
		if ( usage.HasFlag( BufferUsage.CpuRead ) ) {
			storageFlags |= BufferStorageFlags.MapReadBit;
			accessMask |= BufferAccessMask.MapReadBit;
		}
		if ( usage.HasFlag( BufferUsage.CpuWrite ) ) {
			storageFlags |= BufferStorageFlags.MapWriteBit;
			accessMask |= BufferAccessMask.MapWriteBit;
		}

		GL.NamedBufferStorage( handle, (int)size, (nint)null, storageFlags );
		Data = (void*)GL.MapNamedBufferRange( handle, 0, (int)size, accessMask );
		Handle = handle;
	}

	public void* GetData () {
		return Data;
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteBuffer( Handle );
		deleteHandle();
	}

	[Conditional( "DEBUG" )]
	void deleteHandle () {
		Handle = 0;
	}
}
