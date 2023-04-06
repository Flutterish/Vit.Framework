using System.Runtime.InteropServices;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public class VertexBuffer<T> : DisposableVulkanObject<VkBuffer> where T : unmanaged {
	public static readonly uint Stride = (uint)Marshal.SizeOf(default(T));
	public readonly Device Device;
	VkDeviceMemory memory;

	public VertexBuffer ( Device device ) {
		Device = device;
	}

	public unsafe void Allocate ( ReadOnlySpan<T> data ) {
		Free();
		VkBufferCreateInfo info = new() {
			sType = VkStructureType.BufferCreateInfo,
			size = Stride * (ulong)data.Length,
			usage = VkBufferUsageFlags.VertexBuffer,
			sharingMode = VkSharingMode.Exclusive
		};
		Vk.vkCreateBuffer( Device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();

		Vk.vkGetBufferMemoryRequirements( Device, Instance, out var reqs );
		VkMemoryAllocateInfo allocInfo = new() {
			sType = VkStructureType.MemoryAllocateInfo,
			allocationSize = reqs.size,
			memoryTypeIndex = Device.PhysicalDevice.FindMemoryType( reqs.memoryTypeBits, VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent )
		};
		Vk.vkAllocateMemory( Device, &allocInfo, VulkanExtensions.TODO_Allocator, out memory ).Validate();
		Vk.vkBindBufferMemory( Device, this, memory, 0 );

		void* dataPointer;
		Vk.vkMapMemory( Device, memory, 0, info.size, 0, &dataPointer ).Validate();
		data.CopyTo( new Span<T>( dataPointer, data.Length ) );
		Vk.vkUnmapMemory( Device, memory );
	}

	public unsafe void Free () {
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
