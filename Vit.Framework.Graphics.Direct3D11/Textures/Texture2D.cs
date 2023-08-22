﻿using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Vit.Framework.Graphics.Direct3D11.Textures;

public class Texture2D : DisposableObject, ITexture2D {
	public Size2<uint> Size { get; }
	public PixelFormat Format { get; }
	public readonly ID3D11Texture2D Texture;
	public Texture2D ( ID3D11Device device, Size2<uint> size, PixelFormat _format ) {
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
			Usage = ResourceUsage.Dynamic,
			BindFlags = BindFlags.ShaderResource,
			CPUAccessFlags = CpuAccessFlags.Write
		} );
	}

	public void Upload<TPixel> ( ReadOnlySpan<TPixel> data, ID3D11DeviceContext context ) where TPixel : unmanaged {
		Debug.Assert( Format == PixelFormat.Rgba8 );
		var map = context.Map<TPixel>( Texture, 0, 0, MapMode.WriteDiscard );
		data.CopyTo( map );
		context.Unmap( Texture, 0 );
	}

	public ITexture2DView CreateView () {
		return new Texture2DView( Texture.Device, this );
	}

	protected override void Dispose ( bool disposing ) {
		Texture.Dispose();
	}
}
