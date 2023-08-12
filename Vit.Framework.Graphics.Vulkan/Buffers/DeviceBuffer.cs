using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public interface IVulkanDeviceBuffer : IDeviceBuffer {
	void TransferRaw ( ReadOnlySpan<byte> data, uint offset, CommandBuffer commands );
}

public class DeviceBuffer<T> : Buffer, IVulkanDeviceBuffer, IDeviceBuffer<T> where T : unmanaged {
	public uint Stride => UsageFlags.HasFlag( VkBufferUsageFlags.UniformBuffer ) ? IBuffer<T>.UniformBufferStride : IBuffer<T>.Stride;
	HostBuffer<T> stagingBuffer;
	public DeviceBuffer ( Device device, VkBufferUsageFlags flags ) : base( device, flags | VkBufferUsageFlags.TransferDst ) {
		stagingBuffer = new( device, VkBufferUsageFlags.TransferSrc );
	}

	public void AllocateRaw ( uint size, BufferUsage usageHint ) {
		stagingBuffer.AllocateRaw( size );
		base.Allocate( size );
	}

	public void Allocate ( uint size, BufferUsage usageHint ) {
		AllocateRaw( size * Stride, usageHint );
	}

	public void TransferRaw ( ReadOnlySpan<byte> data, uint offset, CommandBuffer commands ) {
		stagingBuffer.UploadRaw( data, offset );
		commands.Copy( stagingBuffer, this, (uint)data.Length, srcOffset: offset, dstOffset: offset );
	}
	public void Transfer ( ReadOnlySpan<T> data, uint offset, CommandBuffer commands ) {
		stagingBuffer.Upload( data, offset );
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
