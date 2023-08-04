using System.Diagnostics;
using System.Runtime.InteropServices;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public abstract class Buffer<T> : DisposableVulkanObject<VkBuffer> where T : unmanaged {
	public static readonly uint Stride = (uint)Marshal.SizeOf( default( T ) );
	public readonly Device Device;
	protected VkDeviceMemory Memory;

	public Buffer ( Device device ) {
		Device = device;
	}

	protected unsafe void Allocate ( ulong length, VkBufferUsageFlags usage, VkSharingMode sharingMode = VkSharingMode.Exclusive ) {
		if ( Instance != VkBuffer.Null )
			Free();

		VkBufferCreateInfo info = new() {
			sType = VkStructureType.BufferCreateInfo,
			size = Stride * length,
			usage = usage,
			sharingMode = sharingMode
		};
		Vk.vkCreateBuffer( Device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();

		Vk.vkGetBufferMemoryRequirements( Device, Instance, out var reqs );
		VkMemoryAllocateInfo allocInfo = new() {
			sType = VkStructureType.MemoryAllocateInfo,
			allocationSize = reqs.size,
			memoryTypeIndex = FindMemoryType( reqs )
		};
		Vk.vkAllocateMemory( Device, &allocInfo, VulkanExtensions.TODO_Allocator, out Memory ).Validate();
		Vk.vkBindBufferMemory( Device, this, Memory, 0 );
	}

	protected abstract uint FindMemoryType ( VkMemoryRequirements requirements );

	protected virtual unsafe void Free () {
		Vk.vkDestroyBuffer( Device, Instance, VulkanExtensions.TODO_Allocator );
		Vk.vkFreeMemory( Device, Memory, VulkanExtensions.TODO_Allocator );
		Instance = VkBuffer.Null;
	}

	protected override void Dispose ( bool disposing ) {
		if ( Instance == VkBuffer.Null )
			return;

		Free();
	}
}
