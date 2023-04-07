using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Buffers;

public class VertexBuffer<T> : Buffer<T> where T : unmanaged {
	public VertexBuffer ( Device device ) : base( device ) { }

	public unsafe void Allocate ( ReadOnlySpan<T> data ) {
		Allocate( (ulong)data.Length, VkBufferUsageFlags.VertexBuffer );
		Transfer( data );
	}
}
