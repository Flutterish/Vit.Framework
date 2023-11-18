using Vit.Framework.Graphics.Rendering.Buffers;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public unsafe class HostBuffer<T> : Buffer, IHostStagingBuffer<T> where T : unmanaged {
	public uint Stride => IBuffer<T>.Stride;
	void* data;
	public HostBuffer ( Device device, VkBufferUsageFlags flags ) : base( device, flags ) { }

	public void* GetData () {
		return data;
	}

	public void AllocateRaw ( uint size ) {
		base.Allocate( size );
		void* dataPointer;
		Vk.vkMapMemory( Device, Memory, 0, size, 0, &dataPointer ).Validate();
		data = dataPointer;
	}

	public void AllocateRaw ( uint size, BufferUsage usageHint ) {
		AllocateRaw( size );
	}

	public void Allocate ( uint size ) {
		AllocateRaw( size * Stride );
	}

	public Span<T> GetDataSpan ( int length, int offset = 0 )
		=> new Span<T>( (T*)data + offset, length );

	void IHostBuffer.Unmap() { }
	public void Unmap () {
		if ( data == null )
			return;

		Vk.vkUnmapMemory( Device, Memory );
		data = null;
	}

	protected override void Free () {
		Unmap();
		base.Free();
	}

	protected override uint FindMemoryType ( VkMemoryRequirements requirements ) {
		return Device.PhysicalDevice.FindMemoryType( requirements.memoryTypeBits, VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent );
	}
}
