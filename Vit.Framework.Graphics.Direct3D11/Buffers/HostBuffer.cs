using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Buffers;

public class HostBuffer<T> : DisposableObject, IHostBuffer<T>, ID3D11BufferHandle where T : unmanaged {
	public static readonly int Stride = Marshal.SizeOf( default( T ) );
	public readonly ID3D11Device Device;
	public readonly ID3D11DeviceContext Context;
	public readonly BindFlags Type;
	public ID3D11Buffer? Handle { get; private set; }
	public HostBuffer ( ID3D11Device device, ID3D11DeviceContext context, BindFlags type ) {
		Device = device;
		Context = context;
		Type = type;
	}

	public void Upload ( ReadOnlySpan<T> data, uint offset = 0 ) {
		var map = Context.Map( Handle!, MapMode.WriteDiscard ); // TODO I dont get why it has to be discrd. Context is an immediate context, not a deferred one.
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

		Device.CreateBuffer( new BufferDescription {
			ByteWidth = (int)size * Stride,
			Usage = ResourceUsage.Dynamic,
			BindFlags = Type,
			CPUAccessFlags = flags
		}, null, out var handle ).Validate();
		Handle = handle;
	}

	protected override void Dispose ( bool disposing ) {
		Handle?.Dispose();
		Handle = null;
	}
}
