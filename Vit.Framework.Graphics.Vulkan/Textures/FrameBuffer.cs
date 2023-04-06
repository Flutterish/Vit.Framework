using Vit.Framework.Graphics.Vulkan.Rendering;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Textures;

public class FrameBuffer : DisposableVulkanObject<VkFramebuffer> {
	public readonly RenderPass RenderPass;
	public readonly VkExtent2D Size;
	public unsafe FrameBuffer ( VkImageView view, VkExtent2D size, RenderPass pass ) {
		Size = size;
		RenderPass = pass;

		var info = new VkFramebufferCreateInfo() {
			sType = VkStructureType.FramebufferCreateInfo,
			renderPass = pass,
			attachmentCount = 1,
			pAttachments = &view,
			width = size.width,
			height = size.height,
			layers = 1
		};

		Vk.vkCreateFramebuffer( pass.Device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyFramebuffer( RenderPass.Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
