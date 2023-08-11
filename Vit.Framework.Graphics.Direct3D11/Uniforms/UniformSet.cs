using System.Diagnostics;
using Vit.Framework.Graphics.Direct3D11.Buffers;
using Vit.Framework.Graphics.Direct3D11.Shaders;
using Vit.Framework.Graphics.Direct3D11.Textures;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Uniforms;

public class UniformSet : DisposableObject, IUniformSet {
	public readonly uint Set;
	public UniformSet ( uint set ) {
		Set = set;
	}

	public Dictionary<uint, ID3D11BufferHandle> ConstantBuffers = new();
	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding, uint offset = 0 ) where T : unmanaged {
		DebugMemoryAlignment.AssertStructAlignment( this, binding, typeof( T ) );
		Debug.Assert( offset == 0 );
		ConstantBuffers[binding] = (ID3D11BufferHandle)buffer;
	}

	public Dictionary<uint, Texture2D> Samplers = new();

	public void SetSampler ( ITexture texture, uint binding ) {
		Samplers[binding] = (Texture2D)texture;
	}

	public void Apply ( ShaderSet shaders, ID3D11DeviceContext context ) {
		var mapping = shaders.UniformMapping;
		
		foreach ( var (originalBinding, buffer) in ConstantBuffers ) {
			var binding = mapping.Bindings[(Set, originalBinding)];

			context.VSSetConstantBuffer( (int)binding, buffer.Handle );
			context.PSSetConstantBuffer( (int)binding, buffer.Handle );
		}
		
		foreach ( var (originalBinding, texture) in Samplers ) {
			var binding = mapping.Bindings[(Set, originalBinding)];

			context.PSSetShaderResource( (int)binding, texture.ResourceView );
			context.PSSetSampler( (int)binding, texture.Sampler );
		}
	}

	protected override void Dispose ( bool disposing ) {
		DebugMemoryAlignment.ClearDebugData( this );
	}
}
