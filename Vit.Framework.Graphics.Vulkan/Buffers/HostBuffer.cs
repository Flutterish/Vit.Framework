using Vit.Framework.Graphics.Rendering.Buffers;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public unsafe class HostBuffer<T> : Buffer, IHostBuffer<T> where T : unmanaged {
	public uint Stride => UsageFlags.HasFlag( VkBufferUsageFlags.UniformBuffer ) ? IBuffer<T>.UniformBufferStride : IBuffer<T>.Stride;
	T* data;
	public HostBuffer ( Device device, VkBufferUsageFlags flags ) : base( device, flags ) { }

	public void AllocateRaw ( uint size ) {
		base.Allocate( size );
		void* dataPointer;
		Vk.vkMapMemory( Device, Memory, 0, size, 0, &dataPointer ).Validate();
		data = (T*)dataPointer;
	}
	public void AllocateRaw ( uint size, BufferUsage usageHint ) {
		AllocateRaw( size );
	}

	public void Allocate ( uint size ) {
		AllocateRaw( size * Stride );
	}
	public void Allocate ( uint size, BufferUsage usageHint ) {
		AllocateRaw( size * Stride );
	}

	public Span<T> GetDataSpan ( int length, int offset = 0 )
		=> new Span<T>( data + offset, length );

	public void UploadRaw ( ReadOnlySpan<byte> data, uint offset = 0 ) {
		data.CopyTo( new Span<byte>( this.data + offset, data.Length ) );
	}

	public void Upload ( ReadOnlySpan<T> data, uint offset ) {
		if ( UsageFlags.HasFlag( VkBufferUsageFlags.UniformBuffer ) ) { // TODO vtable this out
			var stride = IBuffer<T>.UniformBufferStride;
			byte* ptr = ((byte*)this.data) + offset * stride;
			for ( int i = 0; i < data.Length; i++ ) {
				*((T*)ptr) = data[i];
				ptr += stride;
			}
		}
		else {
			data.CopyTo( new Span<T>( this.data + offset, data.Length ) );
		}
	}

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
