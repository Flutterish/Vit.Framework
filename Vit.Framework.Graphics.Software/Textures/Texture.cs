using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Software.Textures;

public interface ISoftwareTexture : ITexture2D, ITexture2DView, IDeviceTexture2D, IStagingTexture2D {
	void CopyTo ( ISoftwareTexture other, AxisAlignedBox2<uint> sourceRect, Point2<uint> destinationOffset );
}

public class Texture<TPixel> : DisposableObject, ISoftwareTexture where TPixel : unmanaged, IPixel<TPixel> {
	public PixelFormat Format { get; }
	public Size2<uint> Size => new( (uint)Image.Width, (uint)Image.Height );
	public Image<TPixel> Image { get; }
	static Configuration configuration = new() { PreferContiguousImageBuffers = true };
	public Texture ( Size2<uint> size, PixelFormat format ) {
		Format = format;
		Image = new( configuration, (int)size.Width, (int)size.Height );
	}

	public Memory<TPixel> Memory => Image.DangerousTryGetSinglePixelMemory( out var memory ) ? memory : throw new Exception( "underlying image is not contigious, somehow" );
	public Span<TPixel> AsSpan () => Memory.Span;
	public Span2D<TPixel> AsSpan2D () => new Span2D<TPixel>( Memory.Span, Image.Width, Image.Height );

	protected override void Dispose ( bool disposing ) {
		Image.Dispose();
	}

	public ITexture2DView CreateView () {
		return this;
	}

	public IDeviceTexture2D Source => this;

	public unsafe void* GetData () {
		Image.DangerousTryGetSinglePixelMemory( out var data );
		return data.Span.Data();
	}

	public void CopyTo ( ISoftwareTexture other, AxisAlignedBox2<uint> sourceRect, Point2<uint> destinationOffset ) {
		other.Upload<TPixel>( AsSpan(), other.Size, sourceRect, destinationOffset );
	}
}
