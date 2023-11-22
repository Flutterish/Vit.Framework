using System.Diagnostics;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public abstract class Buffer : DisposableVulkanObject<VkBuffer> {
	public readonly Device Device;
	protected VkDeviceMemory Memory;

	public Buffer ( Device device, uint size, VkBufferUsageFlags flags, VkSharingMode sharingMode = VkSharingMode.Exclusive ) {
		Device = device;
		allocate( size, flags, sharingMode );
	}

	unsafe void allocate ( ulong length, VkBufferUsageFlags usageFlags, VkSharingMode sharingMode = VkSharingMode.Exclusive ) {
		VkBufferCreateInfo info = new() {
			sType = VkStructureType.BufferCreateInfo,
			size = length,
			usage = usageFlags,
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

	protected unsafe override void Dispose ( bool disposing ) {
		Vk.vkDestroyBuffer( Device, Instance, VulkanExtensions.TODO_Allocator );
		Vk.vkFreeMemory( Device, Memory, VulkanExtensions.TODO_Allocator );
		deleteHandle();
	}

	[Conditional( "DEBUG" )]
	void deleteHandle () {
		Instance = VkBuffer.Null;
	}
}
