using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Textures;

public class ImageTexture : DisposableObject, ITexture {
	public Size2<uint> Size { get; }
	public PixelFormat Format { get; }
	public readonly Image Image;
	public readonly Sampler Sampler;

	public ImageTexture ( Device device, Size2<uint> size, PixelFormat format ) {
		Debug.Assert( format.IsRGBA32 );
		Size = size;
		Format = format;

		Image = new( device );
		Image.Allocate(
			new VkExtent2D { width = size.Width, height = size.Height },
			VkImageUsageFlags.TransferDst | VkImageUsageFlags.TransferSrc | VkImageUsageFlags.Sampled,
			VkFormat.R8g8b8a8Srgb
		);

		Sampler = new( device, 0 );
	}

	protected override void Dispose ( bool disposing ) {
		Sampler.Dispose();
		Image.Dispose();
	}
}
