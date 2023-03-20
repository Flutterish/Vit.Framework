using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;

namespace Vit.Framework.Graphics.OpenGl;

public class OpenGlRenderer : Renderer {
	public OpenGlRenderer () : base( RenderingApi.OpenGl ) { }

	public override INativeIndexBuffer<T> CreateIndexBuffer<T> ()
		=> new Buffers.IndexBuffer<T>();

	public override INativeVertexBuffer<T> CreateVertexBuffer<T> ()
		=> new Buffers.VertexBuffer<T>();
}
