using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Buffers;

public interface ID3D11BufferHandle {
	ID3D11Buffer? Handle { get; }
}

public class Buffer<T> : DisposableObject, IHostBuffer<T>, IDeviceBuffer<T>, ID3D11BufferHandle where T : unmanaged {
	public static readonly int Stride = Marshal.SizeOf( default( T ) );
	public readonly ID3D11Device Device;
	public readonly ID3D11DeviceContext Context;
	public readonly BindFlags Type;
	public ID3D11Buffer? Handle { get; private set; }
	public Buffer ( ID3D11Device device, ID3D11DeviceContext context, BindFlags type ) {
		Device = device;
		Context = context;
		Type = type;
	}

	public void Upload ( ReadOnlySpan<T> data, uint offset = 0 ) {
		var map = Context.Map( Handle!, MapMode.WriteDiscard ); // TODO huh?
		data.CopyTo( MemoryMarshal.Cast<byte, T>( map.AsSpan( (data.Length + (int)offset) * Stride ) )[(int)offset..] );
		Context.Unmap( Handle! );
	}

	public void Allocate ( uint size, BufferUsage usageHint ) {
		Handle?.Dispose();

		CpuAccessFlags flags = 0;
		if ( usageHint.HasFlag( BufferUsage.CpuRead ) )
			flags |= CpuAccessFlags.Read;
		if ( usageHint.HasFlag( BufferUsage.CpuWrite ) )
			flags |= CpuAccessFlags.Write;

		D3DExtensions.Validate( Device.CreateBuffer( new BufferDescription {
			ByteWidth = (int)size * Stride,
			Usage = ResourceUsage.Dynamic,
			BindFlags = Type,
			CPUAccessFlags = flags // TODO separate device and host buffers
		}, null, out var handle ) );
		Handle = handle;
	}

	protected override void Dispose ( bool disposing ) {
		Handle?.Dispose();
		Handle = null;
	}
}
