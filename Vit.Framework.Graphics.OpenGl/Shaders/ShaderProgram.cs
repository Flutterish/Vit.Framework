using System.Collections.Immutable;
using Vit.Framework.Graphics.OpenGl.Buffers;
using Vit.Framework.Graphics.OpenGl.Textures;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
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

	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding = 0, uint offset = 0 ) where T : unmanaged {
		var buf = (Buffer<T>)buffer;

		var index = binding; // TODO not correct, but whatever for now
		GL.BindBufferRange( BufferRangeTarget.UniformBuffer, (int)index, buf.Handle, (nint)offset * Buffer<T>.Stride, Buffer<T>.Stride );
		GL.UniformBlockBinding( (uint)Handle, binding, index );
	}

	public unsafe void SetSampler ( ITexture texture, uint binding = 0 ) {
		GL.BindTextureUnit( (int)binding, ((Texture2D)texture).Handle );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteProgram( Handle );
	}
}
