using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Queues;

public class Queue : VulkanObject<VkQueue> {
	public readonly Device Device;
	public readonly QueueFamily Family;
	public Queue ( VkQueue queue, Device device, QueueFamily family ) {
		Instance = queue;
		Family = family;
		Device = device;
	}

	public static implicit operator QueueFamily ( Queue queue )
		=> queue.Family;
}
