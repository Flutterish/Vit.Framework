using System.Collections.Immutable;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Rendering;

public abstract class GraphicsApi : DisposableObject {
	public readonly GraphicsApiType API;
	public readonly ImmutableArray<RenderingCapabilities> Capabilities;

	public GraphicsApi ( GraphicsApiType api, IEnumerable<RenderingCapabilities> capabilities ) {
		API = api;
		Capabilities = capabilities.ToImmutableArray();
	}
}
