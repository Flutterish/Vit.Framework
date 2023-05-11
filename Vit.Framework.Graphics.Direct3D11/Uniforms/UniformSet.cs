using System.Diagnostics;
using Vit.Framework.Graphics.Direct3D11.Buffers;
using Vit.Framework.Graphics.Direct3D11.Shaders;
using Vit.Framework.Graphics.Direct3D11.Textures;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Uniforms;

public class UniformSet : IUniformSet {
	public readonly uint Set;
	public UniformSet ( uint set ) {
		Set = set;
	}

	public Dictionary<uint, ID3D11Buffer> ConstantBuffers = new();
	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding, uint offset = 0 ) where T : unmanaged {
		Debug.Assert( offset == 0 );
		ConstantBuffers[binding] = ((Buffer<T>)buffer).Handle!;
	}

	public Dictionary<uint, Texture2D> Samplers = new();

	public void SetSampler ( ITexture texture, uint binding ) {
		Samplers[binding] = (Texture2D)texture;
	}

	public void Apply ( ShaderSet shaders, ID3D11DeviceContext context ) {
		var mapping = shaders.UniformMapping;
		
		foreach ( var (originalBinding, handle) in ConstantBuffers ) {
			var binding = mapping.Bindings[(Set, originalBinding)];

			context.VSSetConstantBuffer( (int)binding, handle );
		}
		
		foreach ( var (originalBinding, texture) in Samplers ) {
			var binding = mapping.Bindings[(Set, originalBinding)];

			context.PSSetShaderResource( (int)binding, texture.ResourceView );
			context.PSSetSampler( (int)binding, texture.Sampler );
		}
	}

	public void Dispose () { }
}
