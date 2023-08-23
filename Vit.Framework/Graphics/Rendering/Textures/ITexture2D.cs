using Vit.Framework.Interop;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.Rendering.Textures;

public interface ITexture2D : IDisposable {
	Size2<uint> Size { get; }
	PixelFormat Format { get; }
}
public interface IDeviceTexture2D : ITexture2D {
	ITexture2DView CreateView ();
}
// TODO also host texture when we need that
public unsafe interface IStagingTexture2D : ITexture2D {
	void* GetData ();

	/// <summary>
	/// Uploads data to the texture.
	/// </summary>
	/// <param name="data">The data to upload. Must be tightly packed.</param>
	/// <param name="dataSize">The size of the image the data represents in pixels.</param>
	/// <param name="dataRect">The rectange to copy from data in pixels.</param>
	/// <param name="offset">The offset into this texture in pixels.</param>
	public unsafe void Upload<TPixel> ( ReadOnlySpan<TPixel> data, Size2<uint> dataSize, AxisAlignedBox2<uint> dataRect, Point2<uint> offset ) where TPixel : unmanaged {
		var ptr = (TPixel*)GetData();
		var dataPtr = data.Data();

		dataPtr += dataRect.MinX + dataRect.MinY * dataSize.Width;
		ptr += offset.X + offset.Y * Size.Width;

		for ( int i = 0; i < dataRect.Height; i++ ) {
			new Span<TPixel>( dataPtr, (int)dataRect.Width ).CopyTo( new Span<TPixel>( ptr, (int)dataRect.Width ) );
			dataPtr += dataSize.Width;
			ptr += Size.Width;
		}
	}

	/// <summary>
	/// Uploads data to the texture.
	/// </summary>
	/// <param name="data">The data to upload. Must be tightly packed and the same width as the texture.</param>
	public unsafe void Upload<TPixel> ( ReadOnlySpan<TPixel> data ) where TPixel : unmanaged {
		data.CopyTo( new Span<TPixel>( GetData(), (int)(Size.Width * Size.Height) ) );
	}
}