using Vit.Framework.Graphics.OpenGl.Buffers;
using Vit.Framework.Graphics.OpenGl.Textures;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Uniforms;

public class UniformSet : DisposableObject, IUniformSet {
	UniformLayout layout;
	public UniformSet ( UniformLayout layout ) {
		this.layout = layout;

		UboBuffers = new int[layout.UboCount];
		UboOffsets = new nint[layout.UboCount];
		UboSizes = new nint[layout.UboCount];

		SsboBuffers = new int[layout.SsboCount];
		SsboOffsets = new nint[layout.SsboCount];
		SsboSizes = new nint[layout.SsboCount];

		Textures = new int[layout.SamplerCount];
		Samplers = new int[layout.SamplerCount];
	}

	int[] UboBuffers;
	nint[] UboOffsets;
	nint[] UboSizes;
	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding, uint offset = 0 ) where T : unmanaged {
		DebugMemoryAlignment.AssertStructAlignment( this, binding, typeof(T) );
		var buf = (IGlBuffer)buffer;

		var alignedStride = (buf.Stride + 255) / 256 * 256;
		binding = layout.BindingLookup[binding];
		UboBuffers[binding] = buf.Handle;
		UboOffsets[binding] = (nint)(offset * alignedStride);
		UboSizes[binding] = (nint)buf.Stride;
	}

	int[] SsboBuffers;
	nint[] SsboOffsets;
	nint[] SsboSizes;
	public void SetStorageBufferRaw ( IBuffer buffer, uint binding, uint size, uint offset = 0 ) {
		var buf = (IGlBuffer)buffer;

		binding = layout.BindingLookup[binding];
		SsboBuffers[binding] = buf.Handle;
		SsboOffsets[binding] = (nint)offset;
		SsboSizes[binding] = (nint)size;
	}

	int[] Textures;
	int[] Samplers;
	public void SetSampler ( ITexture2DView texture, ISampler sampler, uint binding ) {
		binding = layout.BindingLookup[binding];
		Textures[binding] = ((Texture2DView)texture).Handle;
		Samplers[binding] = ((Sampler)sampler).Handle;
	}

	public void Apply () {
		GL.BindBuffersRange( BufferRangeTarget.UniformBuffer, layout.FirstUbo, layout.UboCount, UboBuffers, UboOffsets, UboSizes );
		GL.BindBuffersRange( BufferRangeTarget.ShaderStorageBuffer, layout.FirstSsbo, layout.SsboCount, SsboBuffers, SsboOffsets, SsboSizes );
		GL.BindTextures( layout.FirstSampler, layout.SamplerCount, Textures );
		GL.BindSamplers( layout.FirstSampler, layout.SamplerCount, Samplers );
	}

	protected override void Dispose ( bool disposing ) {
		DebugMemoryAlignment.ClearDebugData( this );
	}
}
