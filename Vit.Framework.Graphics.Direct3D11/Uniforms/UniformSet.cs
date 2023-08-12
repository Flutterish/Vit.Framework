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

	public Dictionary<uint, (ID3D11BufferHandle buffer, int offset, int stride)> ConstantBuffers = new();
	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding, uint offset = 0 ) where T : unmanaged {
		DebugMemoryAlignment.AssertStructAlignment( this, binding, typeof( T ) );
		ConstantBuffers[binding] = ((ID3D11BufferHandle)buffer, (int)(offset * IBuffer<T>.UniformBufferStride) / 16, (int)IBuffer<T>.Stride );
	}

	public Dictionary<uint, Texture2D> Samplers = new();

	public void SetSampler ( ITexture texture, uint binding ) {
		Samplers[binding] = (Texture2D)texture;
	}

	[ThreadStatic]
	static int[]? firstConstant;
	[ThreadStatic]
	static int[]? numConstants;
	public void Apply ( ShaderSet shaders, ID3D11DeviceContext ctx ) {
		var mapping = shaders.UniformMapping;

		var context = (ID3D11DeviceContext1)ctx;
		foreach ( var (originalBinding, (buffer, offset, stride)) in ConstantBuffers ) {
			var binding = mapping.Bindings[(Set, originalBinding)];

			(firstConstant ??= new int[1])[0] = offset;
			(numConstants ??= new int[1])[0] = stride;
			context.VSSetConstantBuffer1( (int)binding, buffer.Handle, firstConstant, numConstants );
			context.PSSetConstantBuffer1( (int)binding, buffer.Handle, firstConstant, numConstants );
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
