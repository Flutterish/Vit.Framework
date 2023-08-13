using Vit.Framework.Graphics.Rendering.Buffers;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public class DeviceBuffer<T> : Buffer, IDeviceBuffer<T> where T : unmanaged {
	public uint Stride => IBuffer<T>.Stride;
	public DeviceBuffer ( Device device, VkBufferUsageFlags flags ) : base( device, flags | VkBufferUsageFlags.TransferDst ) {
		
	}

	public void AllocateRaw ( uint size, BufferUsage usageHint ) {
		base.Allocate( size );
	}

	protected override uint FindMemoryType ( VkMemoryRequirements requirements ) {
		return Device.PhysicalDevice.FindMemoryType( requirements.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal );
	}

	protected override void Dispose ( bool disposing ) {
		base.Dispose( disposing );
	}
}
