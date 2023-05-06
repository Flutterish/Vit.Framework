﻿using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Textures;

namespace Vit.Framework.Graphics.Rendering.Uniforms;

/// <summary>
/// A set of uniform values. May be shared across multiple shaders and shader sets.
/// </summary>
/// <remarks>
/// In SPIR-V, this is bound to a given <c>layout(set = #)</c>, and its individual components are bound to <c>layout(binding = #)</c>.
/// </remarks>
public interface IUniformSet : IDisposable {
	/// <summary>
	/// Binds a uniform buffer to this uniform set.
	/// </summary>
	/// <param name="buffer">The uniform buffer containing uniform data.</param>
	/// <param name="binding">The binding to link the uniform buffer to.</param>
	/// <param name="offset">Offset in elements into the uniform buffer.</param>
	void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding, uint offset = 0 ) where T : unmanaged;

	/// <summary>
	/// Binds a sampler for the texture to this uniform set.
	/// </summary>
	/// <param name="texture">The texture to create a sampler for.</param>
	/// <param name="binding">The binding to link the sampler to.</param>
	void SetSampler ( ITexture texture, uint binding );
}