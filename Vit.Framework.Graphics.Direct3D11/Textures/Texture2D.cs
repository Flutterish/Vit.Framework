using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Textures;

public unsafe class Texture2D : DisposableObject, IDeviceTexture2D, IStagingTexture2D {
	public Size2<uint> Size { get; }
	public PixelFormat Format { get; }
	public readonly ID3D11Texture2D Texture;
	public Texture2D ( ID3D11Device device, Size2<uint> size, PixelFormat _format, bool isStaging ) {
		Size = size;
		Format = _format;
		var format = Direct3D11Api.formats[_format];

		Texture = device.CreateTexture2D( new Texture2DDescription {
			Width = (int)size.Width,
			Height = (int)size.Height,
			MipLevels = 1,
			ArraySize = 1,
			Format = format,
			SampleDescription = {
				Count = 1,
				Quality = 0
			},
			Usage = isStaging ? ResourceUsage.Staging : ResourceUsage.Default,
			BindFlags = isStaging ? BindFlags.None : _format.Type == PixelType.Color ? BindFlags.ShaderResource | BindFlags.RenderTarget : BindFlags.DepthStencil,
			CPUAccessFlags = isStaging ? CpuAccessFlags.Write : CpuAccessFlags.None
		} );

		if ( isStaging ) {
			var map = device.ImmediateContext.Map<byte>( Texture, 0, 0, MapMode.Write );
			data = map.Data();
		}
	}

	public ITexture2DView CreateView () {
		return new Texture2DView( Texture.Device, this );
	}

	protected override void Dispose ( bool disposing ) {
		Texture.Dispose();
	}

	void* data;
	public unsafe void* GetData () {
		return data;
	}
}
