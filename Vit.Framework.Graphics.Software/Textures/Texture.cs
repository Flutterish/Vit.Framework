﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Software.Textures;

public class Texture : DisposableObject, ITexture2D, ITexture2DView {
	public PixelFormat Format { get; }
	public Size2<uint> Size => new( (uint)Image.Width, (uint)Image.Height );
	public Image<Rgba32> Image { get; }
	static Configuration configuration = new() { PreferContiguousImageBuffers = true };
	public Texture ( Size2<uint> size, PixelFormat format ) {
		Debug.Assert( format == PixelFormat.Rgba8 );
		Format = format;
		Image = new( configuration, (int)size.Width, (int)size.Height );
	}

	public Memory<Rgba32> Memory => Image.DangerousTryGetSinglePixelMemory( out var memory ) ? memory : throw new Exception( "underlying image is not contigious, somehow" );
	public Span<Rgba32> AsSpan () => Memory.Span;
	public Span2D<Rgba32> AsSpan2D () => new Span2D<Rgba32>( Memory.Span, Image.Width, Image.Height );

	protected override void Dispose ( bool disposing ) {
		Image.Dispose();
	}

	public ITexture2DView CreateView () {
		return this;
	}

	public ITexture2D Source => this;
}
