using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Buffers;

public unsafe class HostBuffer<T> : DisposableObject, IGlBuffer, IHostBuffer<T> where T : unmanaged {
	public uint Stride => Type == BufferTarget.UniformBuffer ? IBuffer<T>.UniformBufferStride : IBuffer<T>.Stride;

	public int Handle { get; }
	public readonly BufferTarget Type;
	public T* Data;
	public HostBuffer ( BufferTarget type ) {
		Type = type;
		Handle = GL.GenBuffer();
	}

	public void Allocate ( uint size, BufferUsage usageHint ) {
		GL.BindBuffer( Type, Handle );

		var length = size * Stride;
		GL.BufferStorage( Type, (int)length, (nint)null, BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit | BufferStorageFlags.ClientStorageBit );
		Data = (T*)GL.MapBufferRange( Type, 0, (int)length, BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit );
	}

	public unsafe void Upload ( ReadOnlySpan<T> data, uint offset = 0 ) {
		if ( Type == BufferTarget.UniformBuffer ) { // TODO vtable this out
			var stride = IBuffer<T>.UniformBufferStride;
			byte* ptr = ((byte*)this.Data) + offset * stride;
			for ( int i = 0; i < data.Length; i++ ) {
				*((T*)ptr) = data[i];
				ptr += stride;
			}
		}
		else {
			data.CopyTo( new Span<T>( this.Data + offset, data.Length ) );
		}
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteBuffer( Handle );
	}
}
