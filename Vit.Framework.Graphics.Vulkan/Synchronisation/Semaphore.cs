using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Synchronisation;

public class Semaphore : DisposableVulkanObject<VkSemaphore> {
	public readonly VkDevice Device;
	public unsafe Semaphore ( VkDevice device ) {
		Device = device;
		var info = new VkSemaphoreCreateInfo() {
			sType = VkStructureType.SemaphoreCreateInfo
		};

		Vk.vkCreateSemaphore( device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroySemaphore( Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
