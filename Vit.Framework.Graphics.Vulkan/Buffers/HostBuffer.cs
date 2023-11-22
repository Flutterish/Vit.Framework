using Vit.Framework.Graphics.Rendering.Buffers;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public unsafe class HostBuffer<T> : Buffer, IHostStagingBuffer<T> where T : unmanaged {
	void* data;
	public HostBuffer ( Device device, uint size, VkBufferUsageFlags flags ) : base( device, size, flags ) {
		void* dataPointer;
		Vk.vkMapMemory( Device, Memory, 0, size, 0, &dataPointer ).Validate();
		data = dataPointer;
	}

	public void* GetData () {
		return data;
	}

	public Span<T> GetDataSpan ( int length, int offset = 0 )
		=> new Span<T>( (T*)data + offset, length );

	protected override uint FindMemoryType ( VkMemoryRequirements requirements ) {
		return Device.PhysicalDevice.FindMemoryType( requirements.memoryTypeBits, VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent );
	}

	protected override void Dispose ( bool disposing ) {
		Vk.vkUnmapMemory( Device, Memory );
		base.Dispose( disposing );
	}
}
