using System.Runtime.InteropServices;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public abstract class Buffer<T> : DisposableVulkanObject<VkBuffer> where T : unmanaged {
	public static readonly uint Stride = (uint)Marshal.SizeOf( default( T ) );
	public readonly Device Device;
	VkDeviceMemory memory;

	public Buffer ( Device device ) {
		Device = device;
	}

	protected unsafe void Allocate ( ulong length, VkBufferUsageFlags usage, VkSharingMode sharingMode = VkSharingMode.Exclusive ) {
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
		Vk.vkAllocateMemory( Device, &allocInfo, VulkanExtensions.TODO_Allocator, out memory ).Validate();
		Vk.vkBindBufferMemory( Device, this, memory, 0 );
	}

	protected virtual uint FindMemoryType ( VkMemoryRequirements requirements ) {
		return Device.PhysicalDevice.FindMemoryType( requirements.memoryTypeBits, VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent );
	}

	protected unsafe void Transfer ( ReadOnlySpan<T> data ) {
		void* dataPointer;
		Vk.vkMapMemory( Device, memory, 0, Stride * (ulong)data.Length, 0, &dataPointer ).Validate();
		data.CopyTo( new Span<T>( dataPointer, data.Length ) );
		Vk.vkUnmapMemory( Device, memory );
	}

	protected unsafe void Free () {
		if ( Instance == VkBuffer.Null )
			return;

		Vk.vkDestroyBuffer( Device, Instance, VulkanExtensions.TODO_Allocator );
		Vk.vkFreeMemory( Device, memory, VulkanExtensions.TODO_Allocator );
		Instance = VkBuffer.Null;
	}

	protected override void Dispose ( bool disposing ) {
		Free();
	}
}
