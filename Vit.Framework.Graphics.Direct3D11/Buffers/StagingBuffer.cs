using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Buffers;

public class StagingBuffer<T> : DisposableObject, IStagingBuffer<T>, ID3D11BufferHandle where T : unmanaged {
	public readonly ID3D11Device Device;
	public readonly ID3D11DeviceContext Context;
	public ID3D11Buffer Handle { get; private set; }
	public ID3D11ShaderResourceView? ResourceView => null;
	public StagingBuffer ( ID3D11Device device, ID3D11DeviceContext context, uint size ) {
		Device = device;
		Context = context;

		Device.CreateBuffer( new BufferDescription {
			ByteWidth = (int)size,
			Usage = ResourceUsage.Staging,
			BindFlags = BindFlags.None,
			CPUAccessFlags = CpuAccessFlags.Write
		}, null, out var handle ).Validate();
		Handle = handle;

		data = Context.Map( handle, MapMode.Write );
	}

	MappedSubresource data;
	public unsafe void* GetData () {
		return (void*)data.DataPointer;
	}

	public unsafe void CopyToRaw ( IHostBuffer buffer, uint length, uint sourceOffset = 0, uint destinationOffset = 0 ) {
		var mappable = (IMappable)buffer;
		var map = mappable.Map();
		this.AsSpan<byte>( (int)sourceOffset, (int)length ).CopyTo( new Span<byte>( (byte*)map.DataPointer + destinationOffset, (int)length ) );
		mappable.Unmap();
	}

	public unsafe void SparseCopyToRaw ( IHostBuffer buffer, uint length, uint _size, uint sourceStride, uint destinationStride, uint sourceOffset = 0, uint destinationOffset = 0 ) {
		var mappable = (IMappable)buffer;

		var map = mappable.Map();
		var ptr = (byte*)map.DataPointer + destinationOffset;
		int size = (int)_size;
		for ( int i = 0; i < length; i += (int)sourceStride ) {
			this.AsSpan<byte>( i + (int)sourceOffset, size ).CopyTo( new Span<byte>( ptr, size ) );
			ptr += destinationStride;
		}
		mappable.Unmap();
	}

	protected override void Dispose ( bool disposing ) {
		Handle.Dispose();
		deleteHandle();
	}

	[Conditional("DEBUG")]
	void deleteHandle () {
		Handle = null!;
	}
}
