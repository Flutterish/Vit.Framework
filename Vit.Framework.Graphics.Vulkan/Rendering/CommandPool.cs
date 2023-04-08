using Vit.Framework.Graphics.Vulkan.Queues;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Rendering;

public class CommandPool : DisposableVulkanObject<VkCommandPool> {
	public readonly VkDevice Device;
	public unsafe CommandPool ( VkDevice device, QueueFamily queue, VkCommandPoolCreateFlags flags = VkCommandPoolCreateFlags.ResetCommandBuffer ) {
		Device = device;
		var info = new VkCommandPoolCreateInfo() {
			sType = VkStructureType.CommandPoolCreateInfo,
			flags = flags,
			queueFamilyIndex = queue.Index
		};

		Vk.vkCreateCommandPool( Device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
	}

	public CommandBuffer CreateCommandBuffer () {
		return new CommandBuffer( this );
	}

	public unsafe void FreeCommandBuffer ( CommandBuffer buffer ) {
		var handle = buffer.Handle;
		Vk.vkFreeCommandBuffers( Device, this, 1, &handle );
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyCommandPool( Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
