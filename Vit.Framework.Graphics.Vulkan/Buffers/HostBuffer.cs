using Vit.Framework.Graphics.Rendering.Buffers;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public unsafe class HostBuffer<T> : Buffer, IHostBuffer<T> where T : unmanaged {
	public uint Stride => UsageFlags.HasFlag( VkBufferUsageFlags.UniformBuffer ) ? IBuffer<T>.UniformBufferStride : IBuffer<T>.Stride;
	T* data;
	public HostBuffer ( Device device, VkBufferUsageFlags flags ) : base( device, flags ) { }

	public unsafe void Allocate ( ulong size ) {
		base.Allocate( size );
		void* dataPointer;
		Vk.vkMapMemory( Device, Memory, 0, size, 0, &dataPointer ).Validate();
		data = (T*)dataPointer;
	}

	public unsafe void Allocate ( ReadOnlySpan<T> data ) {
		Allocate( (ulong)data.Length * Stride );
		data.CopyTo( new Span<T>( this.data, data.Length ) );
	}

	public void AllocateRaw ( uint size, BufferUsage usageHint ) {
		Allocate( (ulong)size );
	}

	void IBuffer<T>.Allocate ( uint size, BufferUsage usageHint ) {
		Allocate( (ulong)size * Stride );
	}

	public Span<T> GetDataSpan ( int length, int offset = 0 )
		=> new Span<T>( data + offset, length );

	public unsafe void Transfer ( ReadOnlySpan<T> data, ulong offset = 0 ) {
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

	public unsafe void Transfer ( in T data, ulong offset = 0 ) {
		*(this.data + offset) = data;
	}

	public void UploadRaw ( ReadOnlySpan<byte> data, uint offset = 0 ) {
		data.CopyTo( new Span<byte>( this.data + offset, data.Length ) );
	}

	void IHostBuffer<T>.Upload ( ReadOnlySpan<T> data, uint offset ) {
		Transfer( data, (ulong)offset );
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
