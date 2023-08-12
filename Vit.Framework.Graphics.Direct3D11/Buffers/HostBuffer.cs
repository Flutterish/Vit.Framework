using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Buffers;

public class HostBuffer<T> : DisposableObject, IHostBuffer<T>, ID3D11BufferHandle where T : unmanaged {
	public uint Stride => Type.HasFlag( BindFlags.ConstantBuffer ) ? IBuffer<T>.UniformBufferStride : IBuffer<T>.Stride;
	public readonly ID3D11Device Device;
	public readonly ID3D11DeviceContext Context;
	public readonly BindFlags Type;
	public ID3D11Buffer? Handle { get; private set; }
	public HostBuffer ( ID3D11Device device, ID3D11DeviceContext context, BindFlags type ) {
		Device = device;
		Context = context;
		Type = type;
	}

	public unsafe void UploadRaw ( ReadOnlySpan<byte> data, uint offset = 0 ) {
		var map = Context.Map( Handle!, MapMode.WriteDiscard );
		data.CopyTo( new Span<byte>( (byte*)map.DataPointer + offset, data.Length ) );
		Context.Unmap( Handle! );
	}

	public unsafe void Upload ( ReadOnlySpan<T> data, uint offset = 0 ) {
		var map = Context.Map( Handle!, MapMode.WriteDiscard ); // TODO I dont get why it has to be discrd. Context is an immediate context, not a deferred one.

		if ( Type.HasFlag( BindFlags.ConstantBuffer ) ) { // TODO vtable this out
			var stride = IBuffer<T>.UniformBufferStride;
			byte* ptr = ((byte*)map.DataPointer) + offset * stride;
			for ( int i = 0; i < data.Length; i++ ) {
				*((T*)ptr) = data[i];
				ptr += stride;
			}
		}
		else {
			data.CopyTo( new Span<T>( (void*)((byte*)map.DataPointer + offset), data.Length ) );
		}
		
		Context.Unmap( Handle! );
	}

	public void AllocateRaw ( uint size, BufferUsage usageHint ) {
		Handle?.Dispose();

		CpuAccessFlags flags = 0;
		if ( usageHint.HasFlag( BufferUsage.CpuRead ) )
			flags |= CpuAccessFlags.Read;
		if ( usageHint.HasFlag( BufferUsage.CpuWrite ) )
			flags |= CpuAccessFlags.Write;

		Device.CreateBuffer( new BufferDescription {
			ByteWidth = (int)size,
			Usage = ResourceUsage.Dynamic,
			BindFlags = Type,
			CPUAccessFlags = flags
		}, null, out var handle ).Validate();
		Handle = handle;
	}

	public void Allocate ( uint size, BufferUsage usageHint ) {
		AllocateRaw( size * Stride, usageHint );
	}

	protected override void Dispose ( bool disposing ) {
		Handle?.Dispose();
		Handle = null;
	}
}
