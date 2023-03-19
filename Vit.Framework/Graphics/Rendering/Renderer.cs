using Vit.Framework.Graphics.Rendering.Buffers;

namespace Vit.Framework.Graphics.Rendering;

public abstract class Renderer {
	public readonly RenderingApi API;

	public Renderer ( RenderingApi api ) {
		API = api;
	}

	public abstract INativeIndexBuffer<T> CreateNativeIndexBuffer<T> () where T : unmanaged;
	public abstract INativeVertexBuffer<T> CreateNativeVertexBuffer<T> () where T : unmanaged;
}
