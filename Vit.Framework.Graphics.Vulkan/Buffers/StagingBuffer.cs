using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public class StagingBuffer<T> : Buffer<T> where T : unmanaged {
	public StagingBuffer ( Device device ) : base( device ) { }

	public unsafe void Allocate ( ReadOnlySpan<T> data ) {
		Allocate( (ulong)data.Length, VkBufferUsageFlags.TransferSrc );
		Transfer( data );
	}
}
