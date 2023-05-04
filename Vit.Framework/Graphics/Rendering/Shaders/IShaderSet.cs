using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Textures;

namespace Vit.Framework.Graphics.Rendering.Shaders;

/// <summary>
/// A set of shader parts, used to create a graphics pipeline.
/// </summary>
public interface IShaderSet : IDisposable {
	IEnumerable<IShaderPart> Parts { get; }

	/// <summary>
	/// Binds a uniform buffer to this shader set.
	/// </summary>
	/// <param name="buffer">The uniform buffer containing uniform data.</param>
	/// <param name="binding">The binding set to link the uniform buffer to.</param>
	/// <param name="offset">Offset in elements into the uniform buffer.</param>
	void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding = 0, uint offset = 0 ) where T : unmanaged;

	/// <summary>
	/// Binds a sampler for the texture to this shader set.
	/// </summary>
	/// <param name="texture">The texture to create a sampler for.</param>
	/// <param name="binding">The binding to link the sampler to.</param>
	void SetSampler ( ITexture texture, uint binding = 0 );
}
