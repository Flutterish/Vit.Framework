using System.Diagnostics;
using Vit.Framework.Graphics.OpenGl.Buffers;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.OpenGl.Textures;

public class PixelBuffer : HostBuffer<byte>, IStagingTexture2D, IGlTexture2D {
	GlTextureType IGlTexture2D.Type => GlTextureType.PixelBuffer;
	public Size2<uint> Size { get; }
	public Graphics.Rendering.Textures.PixelFormat Format { get; }

	public PixelBuffer ( Size2<uint> size, Graphics.Rendering.Textures.PixelFormat format ) : base( BufferTarget.PixelUnpackBuffer ) {
		Debug.Assert( format == Graphics.Rendering.Textures.PixelFormat.Rgba8 );
		
		Size = size;
		Format = format;

		AllocateRaw( sizeof(byte) * 4 * size.Width * size.Height, Graphics.Rendering.Buffers.BufferUsage.CpuWrite | Graphics.Rendering.Buffers.BufferUsage.GpuRead );
	}
}
