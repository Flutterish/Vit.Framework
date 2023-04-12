using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Rendering;

public class RenderPass : DisposableVulkanObject<VkRenderPass> {
	public readonly Device Device;
	public readonly VkFormat ColorFormat;
	public readonly VkFormat DepthFormat;
	public readonly VkSampleCountFlags Samples;
	public unsafe RenderPass ( Device device, VkSampleCountFlags samples, VkFormat colorFormat, VkFormat depthFormat ) {
		ColorFormat = colorFormat;
		DepthFormat = depthFormat;
		Samples = samples;
		Device = device;

		var color = new VkAttachmentDescription() {
			format = colorFormat,
			samples = samples,
			loadOp = VkAttachmentLoadOp.Clear,
			storeOp = VkAttachmentStoreOp.Store,
			stencilLoadOp = VkAttachmentLoadOp.DontCare,
			stencilStoreOp = VkAttachmentStoreOp.DontCare,
			initialLayout = VkImageLayout.Undefined,
			finalLayout = VkImageLayout.ColorAttachmentOptimal
		};
		var colorRef = new VkAttachmentReference() {
			attachment = 0,
			layout = VkImageLayout.ColorAttachmentOptimal
		};

		var depth = new VkAttachmentDescription() {
			format = depthFormat,
			samples = samples,
			loadOp = VkAttachmentLoadOp.Clear,
			storeOp = VkAttachmentStoreOp.DontCare,
			stencilLoadOp = VkAttachmentLoadOp.DontCare,
			stencilStoreOp = VkAttachmentStoreOp.DontCare,
			initialLayout = VkImageLayout.Undefined,
			finalLayout = VkImageLayout.DepthStencilAttachmentOptimal
		};
		var depthRef = new VkAttachmentReference() {
			attachment = 1,
			layout = VkImageLayout.DepthStencilAttachmentOptimal
		};

		var resolve = new VkAttachmentDescription() {
			format = colorFormat,
			samples = VkSampleCountFlags.Count1,
			loadOp = VkAttachmentLoadOp.DontCare,
			storeOp = VkAttachmentStoreOp.Store,
			stencilLoadOp = VkAttachmentLoadOp.DontCare,
			stencilStoreOp = VkAttachmentStoreOp.DontCare,
			initialLayout = VkImageLayout.Undefined,
			finalLayout = VkImageLayout.PresentSrcKHR
		};
		var resolveRef = new VkAttachmentReference() {
			attachment = 2,
			layout = VkImageLayout.ColorAttachmentOptimal
		};

		var attachents = new[] { color, depth, resolve };
		var subpass = new VkSubpassDescription() {
			pipelineBindPoint = VkPipelineBindPoint.Graphics,
			colorAttachmentCount = 1,
			pColorAttachments = &colorRef,
			pDepthStencilAttachment = &depthRef,
			pResolveAttachments = &resolveRef
		};

		var dependency = new VkSubpassDependency() {
			srcSubpass = ~0u,
			dstSubpass = 0,
			srcStageMask = VkPipelineStageFlags.ColorAttachmentOutput | VkPipelineStageFlags.EarlyFragmentTests,
			srcAccessMask = 0,
			dstStageMask = VkPipelineStageFlags.ColorAttachmentOutput | VkPipelineStageFlags.EarlyFragmentTests,
			dstAccessMask = VkAccessFlags.ColorAttachmentWrite | VkAccessFlags.DepthStencilAttachmentWrite
		};

		var info = new VkRenderPassCreateInfo() {
			sType = VkStructureType.RenderPassCreateInfo,
			attachmentCount = (uint)attachents.Length,
			pAttachments = attachents.Data(),
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
