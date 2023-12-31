﻿using SixLabors.ImageSharp.PixelFormats;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Software.Buffers;
using Vit.Framework.Graphics.Software.Textures;

namespace Vit.Framework.Graphics.Software.Uniforms;

public class UniformSet : IUniformSet {
	public Dictionary<uint, (IByteBuffer buffer, uint stride, uint offset)> UniformBuffers = new();
	public void SetUniformBufferRaw ( IBuffer buffer, uint binding, uint size, uint offset = 0 ) {
		//UniformBuffers[binding] = ((IByteBuffer)buffer, IBuffer<T>.Stride, offset * IBuffer<T>.Stride);
	}

	public Dictionary<uint, Texture<Rgba32>> Samplers = new();
	public void SetSampler ( ITexture2DView texture, ISampler sampler, uint binding ) {
		Samplers[binding] = (Texture<Rgba32>)texture;
	}

	public void Free () {
		UniformBuffers.Clear();
		Samplers.Clear();
	}

	public void SetStorageBufferRaw ( IBuffer buffer, uint binding, uint size, uint offset = 0 ) {
		throw new NotImplementedException();
	}
}
