using Vit.Framework.Graphics.OpenGl.Buffers;
using Vit.Framework.Graphics.OpenGl.Shaders;
using Vit.Framework.Graphics.OpenGl.Textures;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Uniforms;

public class UniformSet : DisposableObject, IUniformSet {
	public Dictionary<uint, (IGlBuffer buffer, int stride, uint offset)> Buffers = new();
	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding, uint offset = 0 ) where T : unmanaged {
		DebugMemoryAlignment.AssertStructAlignment( this, binding, typeof(T) );
		var buf = (IGlBuffer)buffer;
		Buffers[binding] = (buf, (int)buf.Stride, offset * buf.Stride);
	}

	public Dictionary<uint, Texture2D> Samplers = new();
	public void SetSampler ( ITexture texture, uint binding ) {
		Samplers[binding] = (Texture2D)texture;
	}

	public void Apply ( uint set, ShaderProgram program ) {
		var mapping = program.UniformMapping;

		foreach ( var (originalBinding, (buffer, stride, offset)) in Buffers ) {
			var binding = mapping.Bindings[(set, originalBinding)];

			var UBOindex = binding;
			GL.BindBufferRange( BufferRangeTarget.UniformBuffer, (int)binding, buffer.Handle, (nint)offset, stride ); // TODO these can be optimised into one call if we know the flat mapping
			GL.UniformBlockBinding( (uint)program.Handle, UBOindex, binding ); // this can be extracted to progam itself
		}

		foreach ( var (originalBinding, texture) in Samplers ) {
			var binding = mapping.Bindings[(set, originalBinding)];

			GL.BindTextureUnit( (int)binding, texture.Handle );
		}
	}

	public void Free () {
		Buffers.Clear();
		Samplers.Clear();
	}

	protected override void Dispose ( bool disposing ) {
		DebugMemoryAlignment.ClearDebugData( this );
	}
}
