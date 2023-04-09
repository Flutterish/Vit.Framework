using System.Collections.Immutable;
using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Rendering;

public abstract class Renderer : DisposableObject {
	public readonly RenderingApi API;
	public readonly ImmutableArray<RenderingCapabilities> Capabilities;

	public Renderer ( RenderingApi api, IEnumerable<RenderingCapabilities> capabilities ) {
		API = api;
		Capabilities = capabilities.ToImmutableArray();
	}

	public abstract Matrix4<T> CreateLeftHandCorrectionMatrix<T> () where T : unmanaged, INumber<T>;
}
