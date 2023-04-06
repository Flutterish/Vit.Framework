using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Queues;

public class QueueFamily : VulkanObject<VkQueueFamilyProperties> {
	public VkQueueFlags SupportedOperations => Instance.queueFlags;
	public uint QueueCount => Instance.queueCount;
	public readonly uint Index;

	public QueueFamily ( VkQueueFamilyProperties properties, uint index ) {
		Instance = properties;
		Index = index;
	}
}
