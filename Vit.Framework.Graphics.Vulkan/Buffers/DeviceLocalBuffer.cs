using Vit.Framework.Graphics.Vulkan.Rendering;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public class DeviceLocalBuffer<T> : Buffer<T> where T : unmanaged {
	StagingBuffer<T> stagingBuffer;
	VkBufferUsageFlags flags;
	public DeviceLocalBuffer ( Device device, VkBufferUsageFlags flags ) : base( device ) {
		stagingBuffer = new( device );
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

	protected override uint FindMemoryType ( VkMemoryRequirements requirements ) {
		return Device.PhysicalDevice.FindMemoryType( requirements.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal );
	}

	protected override void Dispose ( bool disposing ) {
		stagingBuffer.Dispose();
		base.Dispose( disposing );
	}
}
