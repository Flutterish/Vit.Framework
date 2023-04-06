using Vit.Framework.Graphics.Vulkan.Queues;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Rendering;

public class CommandPool : DisposableVulkanObject<VkCommandPool> {
	public readonly VkDevice Device;
	public unsafe CommandPool ( VkDevice device, QueueFamily queue ) {
		Device = device;
		var info = new VkCommandPoolCreateInfo() {
			sType = VkStructureType.CommandPoolCreateInfo,
			flags = VkCommandPoolCreateFlags.ResetCommandBuffer,
			queueFamilyIndex = queue.Index
		};

		Vk.vkCreateCommandPool( Device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
	}

	public CommandBuffer CreateCommandBuffer () {
		return new CommandBuffer( this );
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyCommandPool( Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
