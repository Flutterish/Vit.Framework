using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Memory;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Vit.Framework.Graphics.Direct3D11.Buffers;

public class DeviceBuffer<T> : DisposableObject, IDeviceBuffer<T>, ID3D11BufferHandle where T : unmanaged {
	public readonly ID3D11Device Device;
	public readonly ID3D11DeviceContext Context;
	public ID3D11Buffer Handle { get; private set; }
	public ID3D11ShaderResourceView? ResourceView { get; private set; }
	public DeviceBuffer ( ID3D11Device device, ID3D11DeviceContext context, uint size, BindFlags type ) {
		Device = device;
		Context = context;

		Device.CreateBuffer( new BufferDescription {
			ByteWidth = (int)size,
			Usage = ResourceUsage.Default,
			BindFlags = type,
			CPUAccessFlags = CpuAccessFlags.None,
			MiscFlags = type.HasFlag( BindFlags.ShaderResource ) ? ResourceOptionFlags.BufferAllowRawViews : 0
		}, null, out var handle ).Validate();
		Handle = handle;

		if ( !type.HasFlag( BindFlags.ShaderResource ) )
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
		deleteHandle();
	}

	[Conditional( "DEBUG" )]
	void deleteHandle () {
		Handle = null!;
		ResourceView = null;
	}
}
