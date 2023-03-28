using System.Collections.Immutable;
using Vit.Framework.Allocation;

namespace Vit.Framework.Graphics.Rendering;

public abstract class Renderer : DisposableObject {
	public readonly RenderingApi API;
	public readonly ImmutableArray<RenderingCapabilities> Capabilities;

	public Renderer ( RenderingApi api, IEnumerable<RenderingCapabilities> capabilities ) {
		API = api;
		Capabilities = capabilities.ToImmutableArray();
	}

	//public abstract INativeIndexBuffer<T> CreateIndexBuffer<T> () where T : unmanaged;
	//public abstract INativeVertexBuffer<T> CreateVertexBuffer<T> () where T : unmanaged;
}
