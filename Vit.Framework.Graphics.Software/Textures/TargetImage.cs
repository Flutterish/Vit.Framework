using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Software.Textures;

public class TargetImage : DisposableObject, IFramebuffer {
	public Image<Rgba32> Image { get; }
	public Image<D24S8> DepthStencil { get; }
	static Configuration configuration = new() { PreferContiguousImageBuffers = true };
	public TargetImage ( Size2<uint> size ) {
		Image = new( configuration, (int)size.Width, (int)size.Height );
		DepthStencil = new( configuration, (int)size.Width, (int)size.Height );
	}

	public Memory<Rgba32> Memory => Image.DangerousTryGetSinglePixelMemory( out var memory ) ? memory : throw new Exception( "underlying image is not contigious, somehow" );
	public Span<Rgba32> AsSpan () => Memory.Span;
	public Span2D<Rgba32> AsSpan2D () => new Span2D<Rgba32>( Memory.Span, Image.Width, Image.Height );

	public Memory<D24S8> DepthStencilMemory => DepthStencil.DangerousTryGetSinglePixelMemory( out var memory ) ? memory : throw new Exception( "underlying image is not contigious, somehow" );
	public Span<D24S8> DepthStencilAsSpan () => DepthStencilMemory.Span;
	public Span2D<D24S8> DepthStencilAsSpan2D () => new Span2D<D24S8>( DepthStencilMemory.Span, Image.Width, Image.Height );

	protected override void Dispose ( bool disposing ) {
		Image.Dispose();
		DepthStencil.Dispose();
	}
}
