using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Vit.Framework.Graphics.Direct3D11.Buffers;

public interface IMappable {
	MappedSubresource Map ();
	void Unmap ();
}

public class HostBuffer<T> : DisposableObject, IHostBuffer<T>, ID3D11BufferHandle, IMappable where T : unmanaged {
	public readonly ID3D11Device Device;
	public readonly ID3D11DeviceContext Context;
	public readonly BindFlags Type;
	public ID3D11Buffer? Handle { get; private set; }
	public ID3D11ShaderResourceView? ResourceView { get; private set; }
	public HostBuffer ( ID3D11Device device, ID3D11DeviceContext context, BindFlags type ) {
		Device = device;
		Context = context;
		Type = type;
	}

	public MappedSubresource Map () {
		return Context.Map( Handle!, MapMode.WriteNoOverwrite );
	}

	unsafe void* IHostBuffer.Map () {
		return (void*)Map().DataPointer;
	}
	public void Unmap () {
		Context.Unmap( Handle! );
	}

	public unsafe void UploadRaw ( ReadOnlySpan<byte> data, uint offset = 0 ) {
		var map = Context.Map( Handle!, MapMode.WriteNoOverwrite );
		data.CopyTo( new Span<byte>( (byte*)map.DataPointer + offset, data.Length ) );
		Context.Unmap( Handle! );
	}

	public unsafe void UploadSparseRaw ( ReadOnlySpan<byte> data, uint _size, uint sourceStride, uint descinationStride, uint offset = 0 ) {
		var map = Context.Map( Handle!, MapMode.WriteNoOverwrite );
		var ptr = (byte*)map.DataPointer + offset;
		int size = (int)_size;
		for ( int i = 0; i < data.Length; i += (int)sourceStride ) {
			data.Slice( i, size ).CopyTo( new Span<byte>( ptr, size ) );
			ptr += descinationStride;
		}
		Context.Unmap( Handle! );
	}

	public void AllocateRaw ( uint size, BufferUsage usageHint ) {
		Handle?.Dispose();
		ResourceView?.Dispose();

		CpuAccessFlags flags = 0;
		if ( usageHint.HasFlag( BufferUsage.CpuRead ) )
			flags |= CpuAccessFlags.Read;
		if ( usageHint.HasFlag( BufferUsage.CpuWrite ) )
			flags |= CpuAccessFlags.Write;

		
		Device.CreateBuffer( new BufferDescription {
			ByteWidth = Type == BindFlags.ConstantBuffer ? ((int)size + 15) / 16 * 16 : (int)size,
			Usage = ResourceUsage.Dynamic,
			BindFlags = Type,
			CPUAccessFlags = flags,
			MiscFlags = Type.HasFlag( BindFlags.ShaderResource ) ? ResourceOptionFlags.BufferAllowRawViews : 0
		}, null, out var handle ).Validate();
		Handle = handle;

		if ( !Type.HasFlag( BindFlags.ShaderResource ) )
			return;

		ResourceView = Device.CreateShaderResourceView( Handle, new( 
			Handle, 
			Format.R32_Typeless,
			0,
			(int)(size / 4),
			BufferExtendedShaderResourceViewFlags.Raw
		) );
	}

	protected override void Dispose ( bool disposing ) {
		Handle?.Dispose();
		Handle = null;
	}
}
