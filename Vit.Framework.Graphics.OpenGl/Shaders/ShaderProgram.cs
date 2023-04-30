using System.Collections.Immutable;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Shaders;

public class ShaderProgram : DisposableObject, IShaderSet {
	public IEnumerable<IShaderPart> Parts => Shaders;
	public readonly ImmutableArray<Shader> Shaders;

	public readonly int Handle;
	public ShaderProgram ( IEnumerable<Shader> shaders ) {
		Shaders = shaders.ToImmutableArray();
		Handle = GL.CreateProgram();
		foreach ( var i in shaders ) {
			GL.AttachShader( Handle, i.Handle );
		}
		GL.LinkProgram( Handle );
		GL.GetProgram( Handle, GetProgramParameterName.LinkStatus, out var status );
		if ( status == 0 ) {
			GL.GetProgram( Handle, GetProgramParameterName.InfoLogLength, out var length );
			GL.GetProgramInfoLog( Handle, length, out _, out var info );
			throw new Exception( info );
		}

		foreach ( var i in shaders ) {
			GL.DetachShader( Handle, i.Handle );
		}
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteProgram( Handle );
	}
}
