﻿using Vit.Framework.Graphics.OpenGl.Buffers;
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

		Textures = new int[layout.SamplerCount];
	}

	int[] UboBuffers;
	nint[] UboOffsets;
	nint[] UboSizes;
	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding, uint offset = 0 ) where T : unmanaged {
		DebugMemoryAlignment.AssertStructAlignment( this, binding, typeof(T) );
		var buf = (IGlBuffer)buffer;

		binding = layout.BindingLookup[binding];
		UboBuffers[binding] = buf.Handle;
		UboOffsets[binding] = (nint)(offset * buf.Stride);
		UboSizes[binding] = (nint)(buf.Stride);
	}

	int[] Textures;
	public void SetSampler ( ITexture texture, uint binding ) {
		binding = layout.BindingLookup[binding];
		Textures[binding] = ((Texture2D)texture).Handle;
	}

	public void Apply () {
		GL.BindBuffersRange( BufferRangeTarget.UniformBuffer, layout.FirstUbo, layout.UboCount, UboBuffers, UboOffsets, UboSizes );
		GL.BindTextures( layout.FirstSampler, layout.SamplerCount, Textures );
	}

	protected override void Dispose ( bool disposing ) {
		DebugMemoryAlignment.ClearDebugData( this );
	}
}
