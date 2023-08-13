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

		var uniformInfo = this.CreateUniformInfo();
		UniformMapping = uniformInfo.CreateFlatMapping();
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

		foreach ( var i in LinkedShaders ) {
			GL.DetachShader( Handle, i.Handle );
		}

		foreach ( var i in uniformInfo.Sets ) {
			foreach ( var j in i.Value.Resources.Where( x => x.ResourceType == SPIRVCross.spvc_resource_type.UniformBuffer ) ) {
				var index = UniformMapping.Bindings[(i.Key, j.Binding)];
				GL.UniformBlockBinding( (uint)Handle, index, index );
			}
		}

		var sets = uniformInfo.Sets.Any() ? uniformInfo.Sets.Max( x => x.Key ) + 1 : 0;
		UniformLayouts = new UniformLayout[sets];
		UniformSets = new UniformSet[sets];
		for ( uint i = 0; i < sets; i++ ) {
			UniformLayouts[i] = new( i, uniformInfo.Sets.GetValueOrDefault( i ) ?? new(), UniformMapping );
		}
	}

	public UniformLayout[] UniformLayouts;
	public UniformSet[] UniformSets;
	public IUniformSet? GetUniformSet ( uint set = 0 ) {
		return UniformSets[set];
	}

	public IUniformSet CreateUniformSet ( uint set = 0 ) {
		var value = new UniformSet( UniformLayouts[set] );
		DebugMemoryAlignment.SetDebugData( value, UniformLayouts[set].Type.Resources );
		return value;
	}

	public IUniformSetPool CreateUniformSetPool ( uint set, uint size) {
		return new UniformSetPool( UniformLayouts[set] );
	}

	public void SetUniformSet ( IUniformSet uniforms, uint set = 0 ) {
		UniformSets[set] = (UniformSet)uniforms;
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteProgram( Handle );
	}
}
