using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Validation;

namespace Vit.Framework.Graphics.Rendering.Uniforms;

/// <summary>
/// A set of uniform values for a specific shader set. <br/>
/// A uniform set and its underlying data may not be modified while bound to a command buffer.
/// </summary>
/// <remarks>
/// In SPIR-V, this is bound to a given <c>layout(set = #)</c>, and its individual components are bound to <c>layout(binding = #)</c>.
/// </remarks>
public interface IUniformSet : IDisposable { // TODO use Raw methods
	/// <summary>
	/// Binds a uniform buffer to this uniform set.
	/// </summary>
	/// <param name="buffer">The uniform buffer containing uniform data.</param>
	/// <param name="binding">The binding to link the uniform buffer to.</param>
	/// <param name="size">Size of the uniform data in bytes.</param>
	/// <param name="offset">Offset in bytes into the uniform buffer.</param>
	void SetUniformBufferRaw ( IBuffer buffer, uint binding, uint size, uint offset = 0 );

	/// <summary>
	/// Binds a storage buffer to this uniform set.
	/// </summary>
	/// <param name="buffer">The storage buffer.</param>
	/// <param name="binding">The binding to link the storage buffer to.</param>
	/// <param name="size">Length of the storage buffer in bytes.</param>
	/// <param name="offset">Offset in bytes into the storage buffer.</param>
	void SetStorageBufferRaw ( IBuffer buffer, uint binding, uint size, uint offset = 0 );

	/// <summary>
	/// Binds a sampler for the texture to this uniform set.
	/// </summary>
	/// <param name="texture">The texture view.</param>
	/// <param name="texture">The sampler.</param>
	/// <param name="binding">The binding to link the resources to.</param>
	void SetSampler ( ITexture2DView texture, ISampler sampler, uint binding );
}

public static class IUniformSetExtensions {
	/// <summary>
	/// Binds a uniform buffer to this uniform set.
	/// </summary>
	/// <param name="buffer">The uniform buffer containing uniform data.</param>
	/// <param name="binding">The binding to link the uniform buffer to.</param>
	/// <param name="offset">Offset in elements into the uniform buffer.</param>
	public static void SetUniformBuffer<T> ( this IUniformSet self, IBuffer<T> buffer, uint binding, uint offset = 0, uint alignment = 256 ) where T : unmanaged {
		DebugMemoryAlignment.AssertStructAlignment( self, binding, typeof( T ) );
		self.SetUniformBufferRaw( buffer, binding, IBuffer<T>.Stride, IBuffer<T>.AlignedStride( alignment ) * offset );
	}

	/// <summary>
	/// Binds a storage buffer to this uniform set.
	/// </summary>
	/// <param name="buffer">The storage buffer.</param>
	/// <param name="binding">The binding to link the storage buffer to.</param>
	/// <param name="size">Length of the storage buffer in elements.</param>
	/// <param name="offset">Offset in elements into the storage buffer.</param>
	public static void SetStorageBuffer<T> ( this IUniformSet self, IBuffer<T> buffer, uint binding, uint size, uint offset = 0 ) where T : unmanaged {
		self.SetStorageBufferRaw( buffer, binding, size * IBuffer<T>.Stride, offset * IBuffer<T>.Stride );
	}
}