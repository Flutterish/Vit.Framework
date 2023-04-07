using System.Collections.Immutable;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Rendering;

public abstract class Renderer : DisposableObject {
	public readonly RenderingApi API;
	public readonly ImmutableArray<RenderingCapabilities> Capabilities;

	public Renderer ( RenderingApi api, IEnumerable<RenderingCapabilities> capabilities ) {
		API = api;
		Capabilities = capabilities.ToImmutableArray();
	}
}
