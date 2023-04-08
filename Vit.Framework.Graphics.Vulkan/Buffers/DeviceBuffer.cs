using Vit.Framework.Graphics.Vulkan.Rendering;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public class DeviceBuffer<T> : Buffer<T> where T : unmanaged {
	HostBuffer<T> stagingBuffer;
	VkBufferUsageFlags flags;
	public DeviceBuffer ( Device device, VkBufferUsageFlags flags ) : base( device ) {
		stagingBuffer = new( device, VkBufferUsageFlags.TransferSrc );
		this.flags = flags;
	}

	public unsafe CommandBuffer Allocate ( ReadOnlySpan<T> data, CommandPool pool ) {
		stagingBuffer.Allocate( data );
		Allocate( (ulong)data.Length, flags | VkBufferUsageFlags.TransferDst );

		var commands = pool.CreateCommandBuffer();
		commands.Begin( VkCommandBufferUsageFlags.OneTimeSubmit );
		commands.Copy( stagingBuffer, this, Stride * (uint)data.Length );
		commands.Finish();

		return commands;
	}

	public unsafe void Allocate ( ReadOnlySpan<T> data, CommandBuffer commands ) {
		stagingBuffer.Allocate( data );
		Allocate( (ulong)data.Length, flags | VkBufferUsageFlags.TransferDst );

		commands.Copy( stagingBuffer, this, Stride * (uint)data.Length );
	}

	public void AllocateAndTransfer ( ReadOnlySpan<T> data, CommandPool pool, VkQueue queue ) {
		var copy = Allocate( data, pool );
		copy.Submit( queue );
		Vk.vkQueueWaitIdle( queue );
		pool.FreeCommandBuffer( copy );
	}

	protected override uint FindMemoryType ( VkMemoryRequirements requirements ) {
		return Device.PhysicalDevice.FindMemoryType( requirements.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal );
	}

	protected override void Dispose ( bool disposing ) {
		stagingBuffer.Dispose();
		base.Dispose( disposing );
	}
}
