using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;
using Vit.Framework.Graphics.Software.Buffers;
using Vit.Framework.Graphics.Software.Textures;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Software.Uniforms;

public class UniformSet : DisposableObject, IUniformSet {
	public Dictionary<uint, (IByteBuffer buffer, uint stride, uint offset)> UniformBuffers = new();
	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding = 0, uint offset = 0 ) where T : unmanaged {
		DebugMemoryAlignment.AssertStructAlignment( this, binding, typeof( T ) );
		UniformBuffers[binding] = ((IByteBuffer)buffer, IBuffer<T>.Stride, offset * IBuffer<T>.Stride);
	}

	public Dictionary<uint, Texture> Samplers = new();
	public void SetSampler ( ITexture texture, uint binding = 0 ) {
		Samplers[binding] = (Texture)texture;
	}

	public void Free () {
		UniformBuffers.Clear();
		Samplers.Clear();
	}

	protected override void Dispose ( bool disposing ) {
		DebugMemoryAlignment.ClearDebugData( this );
	}
}
