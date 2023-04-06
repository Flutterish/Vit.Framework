using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Rendering;

public class RenderPass : DisposableVulkanObject<VkRenderPass> {
	public readonly VkDevice Device;
	public unsafe RenderPass ( VkDevice device, VkFormat format ) {
		Device = device;

		var color = new VkAttachmentDescription() {
			format = format,
			samples = VkSampleCountFlags.Count1,
			loadOp = VkAttachmentLoadOp.Clear,
			storeOp = VkAttachmentStoreOp.Store,
			stencilLoadOp = VkAttachmentLoadOp.DontCare,
			stencilStoreOp = VkAttachmentStoreOp.DontCare,
			initialLayout = VkImageLayout.Undefined,
			finalLayout = VkImageLayout.PresentSrcKHR
		};

		var colorRef = new VkAttachmentReference() {
			attachment = 0,
			layout = VkImageLayout.ColorAttachmentOptimal
		};

		var subpass = new VkSubpassDescription() {
			pipelineBindPoint = VkPipelineBindPoint.Graphics,
			colorAttachmentCount = 1,
			pColorAttachments = &colorRef
		};

		var dependency = new VkSubpassDependency() {
			srcSubpass = ~0u,
			dstSubpass = 0,
			srcStageMask = VkPipelineStageFlags.ColorAttachmentOutput,
			srcAccessMask = 0,
			dstStageMask = VkPipelineStageFlags.ColorAttachmentOutput,
			dstAccessMask = VkAccessFlags.ColorAttachmentWrite
		};

		var info = new VkRenderPassCreateInfo() {
			sType = VkStructureType.RenderPassCreateInfo,
			attachmentCount = 1,
			pAttachments = &color,
			subpassCount = 1,
			pSubpasses = &subpass,
			dependencyCount = 1,
			pDependencies = &dependency
		};

		Vk.vkCreateRenderPass( Device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyRenderPass( Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
