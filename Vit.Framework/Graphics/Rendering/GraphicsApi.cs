﻿using System.Collections.Immutable;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Rendering;

/// <summary>
/// A GPU API devoid of any rendering context (such as a window or a logical device), used for arbitrary operations depending on requested <see cref="Capabilities"/>.
/// </summary>
/// <remarks>
/// This is equivalent to <c>VkInstance</c> or header files for OpenGL/D3D.
/// </remarks>
public abstract class GraphicsApi : DisposableObject {
	public readonly GraphicsApiType API;
	public readonly ImmutableArray<RenderingCapabilities> Capabilities;

	public GraphicsApi ( GraphicsApiType api, IEnumerable<RenderingCapabilities> capabilities ) {
		API = api;
		Capabilities = capabilities.ToImmutableArray();
	}
}
