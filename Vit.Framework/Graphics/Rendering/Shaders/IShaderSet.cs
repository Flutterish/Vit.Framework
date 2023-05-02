using Vit.Framework.Graphics.Rendering.Buffers;

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
}
