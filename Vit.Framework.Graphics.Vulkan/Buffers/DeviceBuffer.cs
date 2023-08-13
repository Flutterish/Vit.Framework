using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public interface IVulkanDeviceBuffer : IDeviceBuffer {
	void TransferRaw ( ReadOnlySpan<byte> data, uint offset, CommandBuffer commands );
	void TransferSparseRaw ( ReadOnlySpan<byte> data, uint size, uint stride, uint offset, CommandBuffer commands );
}

public class DeviceBuffer<T> : Buffer, IVulkanDeviceBuffer, IDeviceBuffer<T> where T : unmanaged {
	public uint Stride => IBuffer<T>.Stride;
	HostBuffer<T> stagingBuffer;
	public DeviceBuffer ( Device device, VkBufferUsageFlags flags ) : base( device, flags | VkBufferUsageFlags.TransferDst ) {
		stagingBuffer = new( device, VkBufferUsageFlags.TransferSrc );
	}

	public void AllocateRaw ( uint size, BufferUsage usageHint ) {
		stagingBuffer.AllocateRaw( size );
		base.Allocate( size );
	}

	public void TransferRaw ( ReadOnlySpan<byte> data, uint offset, CommandBuffer commands ) {
		stagingBuffer.UploadRaw( data, offset );
		commands.Copy( stagingBuffer, this, (uint)data.Length, srcOffset: offset, dstOffset: offset );
	}

	public void TransferSparseRaw ( ReadOnlySpan<byte> data, uint size, uint stride, uint offset, CommandBuffer commands ) {
		stagingBuffer.UploadSparseRaw( data, size, stride, offset );
		for ( uint i = 0; i < data.Length; i += size ) {
			commands.Copy( stagingBuffer, this, size, offset, offset ); // TODO merge this into one call
			offset += stride;
		}
	}

	protected override uint FindMemoryType ( VkMemoryRequirements requirements ) {
		return Device.PhysicalDevice.FindMemoryType( requirements.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal );
	}

	protected override void Dispose ( bool disposing ) {
		stagingBuffer.Dispose();
		base.Dispose( disposing );
	}
}
