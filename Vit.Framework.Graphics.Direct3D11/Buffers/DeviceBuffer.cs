using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Buffers;

public class DeviceBuffer<T> : DisposableObject, IDeviceBuffer<T>, ID3D11BufferHandle where T : unmanaged {
	public static readonly int Stride = Marshal.SizeOf( default( T ) );
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
	public void Upload ( ReadOnlySpan<T> data, uint offset, ID3D11DeviceContext context ) {
		data.CopyTo( MemoryMarshal.Cast<byte, T>( this.data.AsSpan( (data.Length + (int)offset) * Stride ) )[(int)offset..] );
		context.CopyResource( Handle!, stagingBuffer! );
	}

	public void Allocate ( uint size, BufferUsage usageHint ) {
		Handle?.Dispose();
		stagingBuffer?.Dispose();

		Device.CreateBuffer( new BufferDescription {
			ByteWidth = (int)size * Stride,
			Usage = ResourceUsage.Default,
			BindFlags = Type,
			CPUAccessFlags = CpuAccessFlags.None
		}, null, out var handle ).Validate();
		Handle = handle;

		Device.CreateBuffer( new BufferDescription {
			ByteWidth = (int)size * Stride,
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
