using Vit.Framework.Allocation;
using Vit.Framework.Graphics.Vulkan.Queues;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan;

public class RenderPass : DisposableObject {
	public readonly VkDevice Device;
	public readonly VkRenderPass Handle;

	RenderPass ( VkDevice device, VkRenderPass handle ) {
		Device = device;
		Handle = handle;
	}

	public static unsafe RenderPass CreateForDrawingToWindow ( VkDevice device, Swapchain.SwapchainParams @params ) {
		VkAttachmentDescription colorAttachment = new() {
			format = @params.Format.format,
			samples = VkSampleCountFlags.Count1,
			loadOp = VkAttachmentLoadOp.Clear,
			storeOp = VkAttachmentStoreOp.Store,
			stencilLoadOp = VkAttachmentLoadOp.DontCare,
			stencilStoreOp = VkAttachmentStoreOp.DontCare,
			initialLayout = VkImageLayout.Undefined,
			finalLayout = VkImageLayout.PresentSrcKHR
		};

		VkAttachmentReference colorAttachmentRef = new() {
			attachment = 0,
			layout = VkImageLayout.ColorAttachmentOptimal
		};

		VkSubpassDescription subpass = new() {
			pipelineBindPoint = VkPipelineBindPoint.Graphics,
			colorAttachmentCount = 1,
			pColorAttachments = &colorAttachmentRef
		};

		VkSubpassDependency dependency = new() {
			srcSubpass = ~0u,
			dstSubpass = 0,
			srcStageMask = VkPipelineStageFlags.ColorAttachmentOutput,
			srcAccessMask = 0,
			dstStageMask = VkPipelineStageFlags.ColorAttachmentOutput,
			dstAccessMask = VkAccessFlags.ColorAttachmentWrite
		};

		VkRenderPassCreateInfo renderPassInfo = new() {
			sType = VkStructureType.RenderPassCreateInfo,
			attachmentCount = 1,
			pAttachments = &colorAttachment,
			subpassCount = 1,
			pSubpasses = &subpass,
			dependencyCount = 1,
			pDependencies = &dependency
		};

		VulkanExtensions.Validate( Vk.vkCreateRenderPass( device, &renderPassInfo, VulkanExtensions.TODO_Allocator, out var renderPass ) );

		return new RenderPass( device, renderPass );
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyRenderPass( Device, Handle, VulkanExtensions.TODO_Allocator );
	}
}
