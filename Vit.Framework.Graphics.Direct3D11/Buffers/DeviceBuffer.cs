using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Buffers;

public interface ID3D11DeviceBuffer : IDeviceBuffer, ID3D11BufferHandle {
	void UploadRaw ( ReadOnlySpan<byte> data, uint offset, ID3D11DeviceContext context );
	void UploadSparseRaw ( ReadOnlySpan<byte> data, uint size, uint stride, uint offset, ID3D11DeviceContext context );
}

public class DeviceBuffer<T> : DisposableObject, IDeviceBuffer<T>, ID3D11DeviceBuffer where T : unmanaged {
	public readonly ID3D11Device Device;
	public readonly ID3D11DeviceContext Context;
	public readonly BindFlags Type;
	public ID3D11Buffer? Handle { get; private set; }
	ID3D11Buffer? stagingBuffer;
	public DeviceBuffer ( ID3D11Device device, ID3D11DeviceContext context, BindFlags type ) {
		Device = device;
		Context = context;
		Type = type;
	}

	MappedSubresource data;
	public unsafe void UploadRaw ( ReadOnlySpan<byte> data, uint offset, ID3D11DeviceContext context ) {
		data.CopyTo( new Span<byte>( (byte*)this.data.DataPointer + offset, data.Length ) );
		context.CopyResource( Handle!, stagingBuffer! );
	}

	public unsafe void UploadSparseRaw ( ReadOnlySpan<byte> data, uint _size, uint stride, uint offset, ID3D11DeviceContext context ) {
		var ptr = (byte*)this.data.DataPointer + offset;
		int size = (int)_size;
		for ( int i = 0; i < data.Length; i += size ) {
			data.Slice( i, size ).CopyTo( new Span<byte>( ptr, size ) );
			ptr += stride;
		}
		context.CopyResource( Handle!, stagingBuffer! );
	}

	public void AllocateRaw ( uint size, BufferUsage usageHint ) {
		Handle?.Dispose();
		stagingBuffer?.Dispose();

		Device.CreateBuffer( new BufferDescription {
			ByteWidth = (int)size,
			Usage = ResourceUsage.Default,
			BindFlags = Type,
			CPUAccessFlags = CpuAccessFlags.None
		}, null, out var handle ).Validate();
		Handle = handle;

		Device.CreateBuffer( new BufferDescription {
			ByteWidth = (int)size,
			Usage = ResourceUsage.Staging,
			BindFlags = BindFlags.None,
			CPUAccessFlags = CpuAccessFlags.Write
		}, null, out stagingBuffer ).Validate();

		data = Context.Map( stagingBuffer, MapMode.Write );
	}

	protected override void Dispose ( bool disposing ) {
		Handle?.Dispose();
		Handle = null;
	}
}
