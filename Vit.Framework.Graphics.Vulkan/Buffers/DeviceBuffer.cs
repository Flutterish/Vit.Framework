using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public class DeviceBuffer<T> : Buffer<T>, IDeviceBuffer<T> where T : unmanaged {
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

	void IBuffer<T>.Allocate ( uint size, BufferUsage usageHint ) {
		stagingBuffer.Allocate( (ulong)size );
		Allocate( (ulong)size, flags | VkBufferUsageFlags.TransferDst );
	}

	public void AllocateAndTransfer ( ReadOnlySpan<T> data, CommandPool pool, VkQueue queue ) {
		var copy = Allocate( data, pool );
		copy.Submit( queue );
		Vk.vkQueueWaitIdle( queue );
		pool.FreeCommandBuffer( copy );
	}

	public void Transfer ( ReadOnlySpan<T> data, ulong offset, CommandBuffer commands ) {
		stagingBuffer.Transfer( data );
		commands.Copy( stagingBuffer, this, Stride * (uint)data.Length, dstOffset: offset );
	}

	protected override uint FindMemoryType ( VkMemoryRequirements requirements ) {
		return Device.PhysicalDevice.FindMemoryType( requirements.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal );
	}

	protected override void Dispose ( bool disposing ) {
		stagingBuffer.Dispose();
		base.Dispose( disposing );
	}
}
