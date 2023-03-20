using Vit.Framework.Graphics.Rendering.Buffers;

namespace Vit.Framework.Graphics.OpenGl.Buffers;

public class VertexBuffer<T> : Buffer<T>, INativeVertexBuffer<T> where T : unmanaged {
	public VertexBuffer () : base( BufferTarget.ArrayBuffer ) { }
}
