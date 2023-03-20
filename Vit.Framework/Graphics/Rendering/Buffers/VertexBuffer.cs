namespace Vit.Framework.Graphics.Rendering.Buffers;

public class VertexBuffer<T> : Buffer<T, INativeVertexBuffer<T>> where T : unmanaged {
	protected override INativeVertexBuffer<T> CreateNativeBuffer ( Renderer renderer )
		=> renderer.CreateVertexBuffer<T>();
}
