using Vit.Framework.Graphics.OpenGl.Buffers;
using Vit.Framework.Graphics.OpenGl.Shaders;
using Vit.Framework.Graphics.OpenGl.Textures;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;

namespace Vit.Framework.Graphics.OpenGl.Uniforms;

public class UniformSet : IUniformSet {
	public Dictionary<uint, (IGlBuffer buffer, int stride, uint offset)> Buffers = new();
	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding, uint offset = 0 ) where T : unmanaged {
		var buf = (Buffer<T>)buffer;
		Buffers[binding] = (buf, Buffer<T>.Stride, offset * (uint)Buffer<T>.Stride);
	}

	public Dictionary<uint, Texture2D> Samplers = new();
	public void SetSampler ( ITexture texture, uint binding ) {
		Samplers[binding] = (Texture2D)texture;
	}

	public void Apply ( ShaderProgram shader ) {
		foreach ( var (binding, (buffer, stride, offset)) in Buffers ) {
			var index = binding; // TODO not correct, but whatever for now
			GL.BindBufferRange( BufferRangeTarget.UniformBuffer, (int)index, buffer.Handle, (nint)offset, stride );
			GL.UniformBlockBinding( (uint)shader.Handle, binding, index );
		}

		foreach ( var (binding, texture) in Samplers ) {
			GL.BindTextureUnit( (int)binding, texture.Handle );
		}
	}

	public void Dispose () { }
}
