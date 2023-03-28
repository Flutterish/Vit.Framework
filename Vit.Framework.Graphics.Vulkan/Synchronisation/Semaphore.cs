using Vit.Framework.Allocation;
using Vit.Framework.Graphics.Rendering.Synchronisation;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Synchronisation;

public class Semaphore : DisposableObject, IGpuBarrier {
	public readonly VkDevice Device;
	public readonly VkSemaphore Handle;

	public Semaphore ( VkDevice device, VkSemaphore handle ) {
		Device = device;
		Handle = handle;
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroySemaphore( Device, Handle, VulkanExtensions.TODO_Allocator );
	}
}

public static class SemaphoreExtensions {
	public static VkSemaphore Semaphore ( this IGpuBarrier? barrier ) => barrier == null ? VkSemaphore.Null : ( (Semaphore)barrier )!.Handle;
}