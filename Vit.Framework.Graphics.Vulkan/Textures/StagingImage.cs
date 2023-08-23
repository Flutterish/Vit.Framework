using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Vulkan.Buffers;
using Vit.Framework.Mathematics;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Textures;

public class StagingImage : HostBuffer<byte>, IStagingTexture2D, IVulkanTexture {
	public VulkanTextureType Type => VulkanTextureType.Buffer;
	public Size2<uint> Size { get; }
	public PixelFormat Format { get; }

	public StagingImage ( Device device, Size2<uint> size, PixelFormat format ) : base( device, VkBufferUsageFlags.TransferSrc ) {
		Debug.Assert( format == PixelFormat.Rgba8 );
		Size = size;
		Format = format;

		AllocateRaw( sizeof(byte) * 4 * size.Width * size.Height );
	}
}
