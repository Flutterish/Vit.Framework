using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;

namespace Vit.Framework.Graphics.Rendering.Shaders;

/// <summary>
/// A set of shader parts, used to create a graphics pipeline.
/// </summary>
public interface IShaderSet : IDisposable {
	IEnumerable<IShaderPart> Parts { get; }

	/// <summary>
	/// Gets the currently bound uniform set.
	/// </summary>
	/// <remarks>
	/// If no uniform set is bound, it will automatically be created.
	/// </remarks>
	IUniformSet GetUniformSet ( uint set = 0 );

	/// <summary>
	/// Creates a new uniform set appropriate for binding to this shader set.
	/// </summary>
	IUniformSet CreateUniformSet ( uint set = 0 );

	/// <summary>
	/// Binds a uniform set to the shader set.
	/// </summary>
	void SetUniformSet ( IUniformSet uniforms, uint set = 0 );

	/// <inheritdoc cref="IUniformSet.SetUniformBuffer{T}(IBuffer{T}, uint, uint)"/>
	/// <param name="set">The uniform set to configure.</param>
	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding = 0, uint offset = 0, uint set = 0 ) where T : unmanaged {
		GetUniformSet( set ).SetUniformBuffer( buffer, binding, offset );
	}

	/// <inheritdoc cref="IUniformSet.SetSampler(ITexture, uint)"/>
	/// <param name="set">The uniform set to configure.</param>
	public void SetSampler ( ITexture texture, uint binding = 0, uint set = 0 ) {
		GetUniformSet( set ).SetSampler( texture, binding );
	}
}

public static class IShaderSetExtensions {
	public static IEnumerable<uint> GetUniformSetIndices ( this IShaderSet shaders ) {
		return shaders.Parts.SelectMany( x => x.ShaderInfo.Uniforms.Sets.Keys ).Distinct().OrderBy( x => x );
	}

	public static UniformSetInfo CreateUniformSetInfo ( this IShaderSet shaders, uint set ) {
		UniformSetInfo info = new();

		var parts = shaders.Parts.Select( x => x.ShaderInfo );
		var uniforms = parts.SelectMany<ShaderInfo, UniformResourceInfo>( x => x.Uniforms.Sets.TryGetValue( set, out var info ) ? info.Resources : Array.Empty<UniformResourceInfo>() );
		foreach ( var i in uniforms.GroupBy( x => x.Binding ) ) {
			var rep = i.First(); // TODO check if they all agree on type
			info.Resources.Add( new() {
				Name = rep.Name,
				Type = rep.Type,
				Binding = rep.Binding,
				Stages = i.SelectMany( x => x.Stages ).ToHashSet()
			} );
		}

		return info;
	}
}