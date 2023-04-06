using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Synchronisation;

public class Fence : DisposableVulkanObject<VkFence> {
	public readonly VkDevice Device;
	public unsafe Fence ( VkDevice device, bool signaled = false ) {
		Device = device;
		var info = new VkFenceCreateInfo() {
			sType = VkStructureType.FenceCreateInfo,
			flags = signaled ? VkFenceCreateFlags.Signaled : VkFenceCreateFlags.None
		};

		Vk.vkCreateFence( device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
	}

	public void Wait () {
		Vk.vkWaitForFences( Device, 1, ref Instance, true, ulong.MaxValue );
	}

	public void Reset () {
		Vk.vkResetFences( Device, 1, ref Instance );
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyFence( Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
