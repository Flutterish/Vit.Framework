using Vit.Framework.Graphics.Rendering.Buffers;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public class DeviceBuffer<T> : Buffer, IDeviceBuffer<T> where T : unmanaged {
	public DeviceBuffer ( Device device, uint size, VkBufferUsageFlags flags ) : base( device, size, flags | VkBufferUsageFlags.TransferDst ) {
		
	}

	protected override uint FindMemoryType ( VkMemoryRequirements requirements ) {
		return Device.PhysicalDevice.FindMemoryType( requirements.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal );
	}
}
