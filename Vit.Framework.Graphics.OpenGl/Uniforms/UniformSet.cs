using Vit.Framework.Graphics.OpenGl.Buffers;
using Vit.Framework.Graphics.OpenGl.Shaders;
using Vit.Framework.Graphics.OpenGl.Textures;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;

namespace Vit.Framework.Graphics.OpenGl.Uniforms;

public class UniformSet : IUniformSet {
	public readonly uint Set;
	public UniformSet ( uint set ) {
		Set = set;
	}

	public Dictionary<uint, (IGlBuffer buffer, int stride, uint offset)> Buffers = new();
	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding, uint offset = 0 ) where T : unmanaged {
		var buf = (Buffer<T>)buffer;
		Buffers[binding] = (buf, Buffer<T>.Stride, offset * (uint)Buffer<T>.Stride);
	}

	public Dictionary<uint, Texture2D> Samplers = new();
	public void SetSampler ( ITexture texture, uint binding ) {
		Samplers[binding] = (Texture2D)texture;
	}

	public void Apply ( ShaderProgram program ) {
		var mapping = program.UniformMapping;

		foreach ( var (originalBinding, (buffer, stride, offset)) in Buffers ) {
			var binding = mapping.Bindings[(Set, originalBinding)];

			var UBOindex = binding;
			GL.BindBufferRange( BufferRangeTarget.UniformBuffer, (int)binding, buffer.Handle, (nint)offset, stride );
			GL.UniformBlockBinding( (uint)program.Handle, UBOindex, binding );
		}

		foreach ( var (originalBinding, texture) in Samplers ) {
			var binding = mapping.Bindings[(Set, originalBinding)];

			GL.BindTextureUnit( (int)binding, texture.Handle );
		}
	}

	public void Dispose () { }
}
