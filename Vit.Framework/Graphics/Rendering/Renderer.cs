using Vit.Framework.Graphics.Rendering.Buffers;

namespace Vit.Framework.Graphics.Rendering;

public abstract class Renderer {
	public readonly RenderingApi API;

	public Renderer ( RenderingApi api ) {
		API = api;
	}

	public abstract INativeIndexBuffer<T> CreateIndexBuffer<T> () where T : unmanaged;
	public abstract INativeVertexBuffer<T> CreateVertexBuffer<T> () where T : unmanaged;
}
