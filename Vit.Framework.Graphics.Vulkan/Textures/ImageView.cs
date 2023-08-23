using Vit.Framework.Graphics.Rendering.Textures;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Textures;

public class ImageView : DisposableVulkanObject<VkImageView>, ITexture2DView {
	public Device Device => ((Image)Source).Device;
	public IDeviceTexture2D Source { get; }
	public unsafe ImageView ( Image source, VkFormat format, VkImageAspectFlags aspect, uint mipMapLevels ) {
		Source = source;

		var viewInfo = new VkImageViewCreateInfo() {
			sType = VkStructureType.ImageViewCreateInfo,
			image = source,
			viewType = VkImageViewType.Image2D,
			format = format,
			subresourceRange = {
				aspectMask = aspect,
				baseMipLevel = 0,
				levelCount = mipMapLevels,
				baseArrayLayer = 0,
				layerCount = 1
			}
		};

		Vk.vkCreateImageView( Device, &viewInfo, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyImageView( Device, this, VulkanExtensions.TODO_Allocator );
	}
}
