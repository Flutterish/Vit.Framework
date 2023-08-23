﻿using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vit.Framework.Interop;
using Vit.Framework.Memory;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Textures;

public class FrameBuffer : DisposableObject, IFramebuffer {
	protected VkFramebuffer Instance;
	public VkFramebuffer Handle => Instance;

	public readonly RenderPass RenderPass;
	public readonly VkExtent2D Size;

	public unsafe FrameBuffer ( VkImageView[] attachements, VkExtent2D size, RenderPass pass ) {
		Size = size;
		RenderPass = pass;

		var info = new VkFramebufferCreateInfo() {
			sType = VkStructureType.FramebufferCreateInfo,
			renderPass = pass,
			attachmentCount = (uint)attachements.Length,
			pAttachments = attachements.Data(),
			width = size.width,
			height = size.height,
			layers = 1
		};

		Vk.vkCreateFramebuffer( pass.Device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
	}

	public static implicit operator VkFramebuffer ( FrameBuffer obj ) => obj.Instance;

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyFramebuffer( RenderPass.Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
