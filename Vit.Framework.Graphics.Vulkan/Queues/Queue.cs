using Vit.Framework.Graphics.Rendering.Queues;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Queues;

public class Queue : IQueue {
	public readonly VkDevice Device;
	public readonly VkQueue Handle;
	public readonly uint FamilyIndex;
	public readonly uint Index;
	public readonly VkCommandPool CommandPool;

	public Queue ( VkDevice device, uint familyIndex, uint index, VkCommandPool pool ) {
		Vk.vkGetDeviceQueue( device, familyIndex, index, out Handle );
		CommandPool = pool;
	}

	public unsafe ICommandBuffer CreateCommandBuffer () {
		VkCommandBufferAllocateInfo bufferInfo = new() {
			sType = VkStructureType.CommandBufferAllocateInfo,
			commandPool = CommandPool,
			level = VkCommandBufferLevel.Primary,
			commandBufferCount = 1
		};

		VulkanExtensions.Validate( Vk.vkAllocateCommandBuffers( Device, &bufferInfo, out var commandBuffer ) );
		return new CommandBuffer( commandBuffer );
	}
}
