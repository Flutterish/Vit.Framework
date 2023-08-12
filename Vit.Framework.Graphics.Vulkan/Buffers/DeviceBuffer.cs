using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public class DeviceBuffer<T> : Buffer<T>, IDeviceBuffer<T> where T : unmanaged {
	HostBuffer<T> stagingBuffer;
	public DeviceBuffer ( Device device, VkBufferUsageFlags flags ) : base( device, flags | VkBufferUsageFlags.TransferDst ) {
		stagingBuffer = new( device, VkBufferUsageFlags.TransferSrc );
	}

	public unsafe CommandBuffer Allocate ( ReadOnlySpan<T> data, CommandPool pool ) {
		stagingBuffer.Allocate( data );
		base.Allocate( (ulong)data.Length );

		var commands = pool.CreateCommandBuffer();
		commands.Begin( VkCommandBufferUsageFlags.OneTimeSubmit );
		commands.Copy( stagingBuffer, this, Stride * (uint)data.Length );
		commands.Finish();

		return commands;
	}

	public unsafe void Allocate ( ReadOnlySpan<T> data, CommandBuffer commands ) {
		stagingBuffer.Allocate( data );
		base.Allocate( (ulong)data.Length );

		commands.Copy( stagingBuffer, this, Stride * (uint)data.Length );
	}

	void IBuffer<T>.Allocate ( uint size, BufferUsage usageHint ) {
		stagingBuffer.Allocate( (ulong)size );
		base.Allocate( (ulong)size );
	}

	public void AllocateAndTransfer ( ReadOnlySpan<T> data, CommandPool pool, VkQueue queue ) {
		var copy = Allocate( data, pool );
		copy.Submit( queue );
		Vk.vkQueueWaitIdle( queue );
		pool.FreeCommandBuffer( copy );
	}

	public void Transfer ( ReadOnlySpan<T> data, ulong offset, CommandBuffer commands ) {
		stagingBuffer.Transfer( data, offset );
		commands.Copy( stagingBuffer, this, Stride * (uint)data.Length, srcOffset: offset * Stride, dstOffset: offset * Stride );
	}

	protected override uint FindMemoryType ( VkMemoryRequirements requirements ) {
		return Device.PhysicalDevice.FindMemoryType( requirements.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal );
	}

	protected override void Dispose ( bool disposing ) {
		stagingBuffer.Dispose();
		base.Dispose( disposing );
	}
}
