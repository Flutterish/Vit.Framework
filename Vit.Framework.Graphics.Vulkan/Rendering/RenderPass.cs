﻿using System.Runtime.CompilerServices;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Rendering;

public class RenderPass : DisposableVulkanObject<VkRenderPass> {
	public readonly Device Device;
	public readonly VkFormat ColorFormat;
	public readonly VkFormat AttachmentFormat; // TODO get rid of this. this is only used by the swapchain
	public readonly VkSampleCountFlags Samples;
	public unsafe RenderPass ( Device device, VkSampleCountFlags samples, VkFormat colorFormat, VkFormat attachmentFormat ) {
		ColorFormat = colorFormat;
		AttachmentFormat = attachmentFormat;
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
			format = attachmentFormat,
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

		var attachments = new[] { color, depth, resolve };
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
			attachmentCount = (uint)attachments.Length,
			pAttachments = attachments.Data(),
			subpassCount = 1,
			pSubpasses = &subpass,
			dependencyCount = 1,
			pDependencies = &dependency
		};

		Vk.vkCreateRenderPass( Device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
	}

	public unsafe RenderPass ( Device device, IEnumerable<IDeviceTexture2D> _attachments, IDeviceTexture2D? depthStencilAttachment ) {
		Device = device;
		Samples = VkSampleCountFlags.Count1;

		var colorCount = _attachments.Count();
		var attachments = new VkAttachmentDescription[colorCount + (depthStencilAttachment == null ? 0 : 1)];
		var references = new VkAttachmentReference[attachments.Length];
		uint i = 0;
		foreach ( var color in _attachments ) {
			attachments[i] = new VkAttachmentDescription() {
				format = VulkanApi.formats[color.Format],
				samples = VkSampleCountFlags.Count1,
				loadOp = VkAttachmentLoadOp.Clear,
				storeOp = VkAttachmentStoreOp.Store,
				stencilLoadOp = VkAttachmentLoadOp.DontCare,
				stencilStoreOp = VkAttachmentStoreOp.DontCare,
				initialLayout = VkImageLayout.Undefined,
				finalLayout = VkImageLayout.ShaderReadOnlyOptimal
			};
			references[i] = new VkAttachmentReference() {
				attachment = i,
				layout = VkImageLayout.ColorAttachmentOptimal
			};
			i++;
		}

		if ( depthStencilAttachment != null ) {
			attachments[i] = new VkAttachmentDescription() {
				format = VulkanApi.formats[depthStencilAttachment.Format],
				samples = VkSampleCountFlags.Count1,
				loadOp = VkAttachmentLoadOp.Clear,
				storeOp = VkAttachmentStoreOp.DontCare,
				stencilLoadOp = VkAttachmentLoadOp.DontCare,
				stencilStoreOp = VkAttachmentStoreOp.DontCare,
				initialLayout = VkImageLayout.Undefined,
				finalLayout = VkImageLayout.DepthStencilAttachmentOptimal
			};
			references[i] = new VkAttachmentReference() {
				attachment = i,
				layout = VkImageLayout.DepthStencilAttachmentOptimal
			};
		}

		var subpass = new VkSubpassDescription() {
			pipelineBindPoint = VkPipelineBindPoint.Graphics,
			colorAttachmentCount = (uint)colorCount,
			pColorAttachments = references.Data(),
			pDepthStencilAttachment = depthStencilAttachment == null ? null : (VkAttachmentReference*)Unsafe.AsPointer( ref references[^1] )
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
			attachmentCount = (uint)attachments.Length,
			pAttachments = attachments.Data(),
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
