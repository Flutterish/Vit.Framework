using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Buffers;

public unsafe class HostBuffer<T> : DisposableObject, IGlBuffer, IHostStagingBuffer<T> where T : unmanaged {
	public int Handle { get; private set; }
	public void* Data;
	public HostBuffer ( uint size, BufferTarget type ) {
		Handle = GL.GenBuffer();

		GL.BindBuffer( type, Handle );
		GL.BufferStorage( type, (int)size, (nint)null, BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit | BufferStorageFlags.ClientStorageBit );
		Data = (void*)GL.MapBufferRange( type, 0, (int)size, BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit );
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
