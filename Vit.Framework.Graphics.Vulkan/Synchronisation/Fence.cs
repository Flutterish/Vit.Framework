using Vit.Framework.Allocation;
using Vit.Framework.Graphics.Rendering.Synchronisation;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Synchronisation;

public class Fence : DisposableObject, ICpuBarrier {
	public readonly VkDevice Device;
	public readonly VkFence Handle;

	public Fence ( VkDevice device, VkFence handle ) {
		Device = device;
		Handle = handle;
	}

	public unsafe void Wait () {
		var handle = Handle;
		Vk.vkWaitForFences( Device, 1, &handle, true, ulong.MaxValue );
	}

	public unsafe void Reset () {
		var handle = Handle;
		Vk.vkResetFences( Device, 1, &handle );
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyFence( Device, Handle, VulkanExtensions.TODO_Allocator );
	}
}

public static class FenceExtensions {
	public static VkFence Fence ( this ICpuBarrier? barrier ) => barrier == null ? VkFence.Null : ( (Fence)barrier )!.Handle;
}