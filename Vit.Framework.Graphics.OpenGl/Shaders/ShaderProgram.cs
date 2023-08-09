using System.Collections.Immutable;
using Vit.Framework.Graphics.OpenGl.Uniforms;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Shaders;

public class ShaderProgram : DisposableObject, IShaderSet {
	public IEnumerable<IShaderPart> Parts => Shaders;
	public readonly ImmutableArray<UnlinkedShader> Shaders;
	public readonly ImmutableArray<Shader> LinkedShaders;

	public readonly int Handle;
	public readonly UniformFlatMapping UniformMapping;
	public ShaderProgram ( IEnumerable<UnlinkedShader> shaders ) {
		Shaders = shaders.ToImmutableArray();

		UniformMapping = this.CreateUniformInfo().CreateFlatMapping();
		LinkedShaders = shaders.Select( x => x.GetShader( UniformMapping ) ).ToImmutableArray();

		Handle = GL.CreateProgram();
		foreach ( var i in LinkedShaders ) {
			GL.AttachShader( Handle, i.Handle );
		}
		GL.LinkProgram( Handle );
		GL.GetProgram( Handle, GetProgramParameterName.LinkStatus, out var status );
		if ( status == 0 ) {
			GL.GetProgram( Handle, GetProgramParameterName.InfoLogLength, out var length );
			GL.GetProgramInfoLog( Handle, length, out _, out var info );
			throw new Exception( info );
		}

		GL.GetProgram( Handle, GetProgramParameterName.ActiveUniformBlocks, out var uboCount );
		var pairs = new RentedArray<(uint set, uint bining)>( uboCount );
		for ( int i = 0; i < uboCount; i++ ) {
			GL.GetActiveUniformBlock( Handle, i, ActiveUniformBlockParameter.UniformBlockBinding, out var binding );
			var key = UniformMapping.Bindings.First( x => x.Value == binding ).Key;
			pairs[i] = key;
		}
		for ( uint i = 0; i < uboCount; i++ ) {
			UniformMapping.Bindings[pairs[i]] = i;
		}

		foreach ( var i in LinkedShaders ) {
			GL.DetachShader( Handle, i.Handle );
		}
	}

	public Dictionary<uint, UniformSet> UniformSets = new();
	public IUniformSet GetUniformSet ( uint set = 0 ) {
		if ( !UniformSets.TryGetValue( set, out var value ) ) {
			UniformSets.Add( set, value = new( set ) );
			DebugMemoryAlignment.SetDebugData( value, set, this );
		}

		return value;
	}

	public IUniformSet CreateUniformSet ( uint set = 0 ) {
		var value = new UniformSet( set );
		DebugMemoryAlignment.SetDebugData( value, set, this );
		return value;
	}

	public void SetUniformSet ( IUniformSet uniforms, uint set = 0 ) {
		UniformSets[set] = (UniformSet)uniforms;
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteProgram( Handle );
	}
}
