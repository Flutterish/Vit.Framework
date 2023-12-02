using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Rendering.Uniforms;

namespace Vit.Framework.Graphics.Rendering.Shaders;

/// <summary>
/// A set of shader parts, used to create a graphics pipeline.
/// </summary>
public interface IShaderSet : IDisposable {
	IEnumerable<IShaderPart> Parts { get; }

	/// <summary>
	/// Creates a pool of <see cref="IUniformSet"/>s.
	/// </summary>
	IUniformSetPool CreateUniformSetPool ( uint set, uint size );

	/// <summary>
	/// Binds a uniform set to the shader set.
	/// </summary>
	void SetUniformSet ( IUniformSet uniforms, uint set = 0 );
}

public static class IShaderSetExtensions {
	public static UniformInfo CreateUniformInfo ( this IShaderSet shaders ) {
		var info = new UniformInfo();
		foreach ( var i in shaders.GetUniformSetIndices() ) {
			info.Sets.Add( i, shaders.CreateUniformSetInfo( i ) );
		}

		return info;
	}

	public static IEnumerable<uint> GetUniformSetIndices ( this IShaderSet shaders ) {
		return shaders.Parts.SelectMany( x => x.ShaderInfo.Uniforms.Sets.Keys ).Distinct().OrderBy( x => x );
	}

	public static (IUniformSet set, IUniformSetPool pool) CreateUniformSet ( this IShaderSet shaders, uint set = 0 ) {
		var pool = shaders.CreateUniformSetPool( set, 1 );
		return (pool.Rent(), pool);
	}

	public static UniformSetInfo CreateUniformSetInfo ( this IShaderSet shaders, uint set ) {
		return shaders.Parts.Select( x => x.ShaderInfo ).CreateUniformSetInfo( set );
	}

	public static UniformSetInfo CreateUniformSetInfo ( this IEnumerable<ShaderInfo> parts, uint set ) {
		UniformSetInfo info = new();

		var uniforms = parts.SelectMany<ShaderInfo, UniformResourceInfo>( x => x.Uniforms.Sets.TryGetValue( set, out var info ) ? info.Resources : Array.Empty<UniformResourceInfo>() );
		foreach ( var i in uniforms.GroupBy( x => x.Binding ) ) {
			var rep = i.First(); // TODO check if they all agree on type
			info.Resources.Add( new() {
				ResourceType = rep.ResourceType,
				Name = rep.Name,
				Type = rep.Type,
				Binding = rep.Binding,
				Stages = i.SelectMany( x => x.Stages ).ToHashSet(),
				Id = uint.MaxValue
			} );
		}

		return info;
	}
}