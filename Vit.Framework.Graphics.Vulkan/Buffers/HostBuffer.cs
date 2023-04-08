using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public unsafe class HostBuffer<T> : Buffer<T> where T : unmanaged {
	T* data;
	VkBufferUsageFlags flags;
	public HostBuffer ( Device device, VkBufferUsageFlags flags ) : base( device ) {
		this.flags = flags;
	}

	public unsafe void Allocate ( ulong count ) {
		Allocate( count, flags );
		void* dataPointer;
		Vk.vkMapMemory( Device, Memory, 0, Stride * count, 0, &dataPointer ).Validate();
		data = (T*)dataPointer;
	}

	public unsafe void Allocate ( ReadOnlySpan<T> data ) {
		Allocate( (ulong)data.Length );
		data.CopyTo( new Span<T>( this.data, data.Length ) );
	}

	public unsafe void Transfer ( ReadOnlySpan<T> data, ulong offset = 0 ) {
		data.CopyTo( new Span<T>( this.data + offset, data.Length ) );
	}

	public unsafe void Transfer ( in T data, ulong offset = 0 ) {
		*(this.data + offset) = data;
	}

	protected override void Free () {
		Vk.vkUnmapMemory( Device, Memory );
		base.Free();
	}

	protected override uint FindMemoryType ( VkMemoryRequirements requirements ) {
		return Device.PhysicalDevice.FindMemoryType( requirements.memoryTypeBits, VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent );
	}
}
