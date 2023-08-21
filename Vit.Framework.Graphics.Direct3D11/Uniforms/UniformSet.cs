using Vit.Framework.Graphics.Direct3D11.Buffers;
using Vit.Framework.Graphics.Direct3D11.Textures;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Uniforms;

public class UniformSet : DisposableObject, IUniformSet {
	UniformLayout layout;
	public UniformSet ( UniformLayout layout ) {
		this.layout = layout;

		ConstantBufferHandles = new ID3D11BufferHandle[layout.ConstantBufferCount];
		ConstantBuffers = new ID3D11Buffer[layout.ConstantBufferCount];
		ConstantBuffersOffsets = new int[layout.ConstantBufferCount];
		ConstantBuffersSizes = new int[layout.ConstantBufferCount];

		SamplerResources = new ID3D11ShaderResourceView[layout.SamplerCount];
		SamplerStates = new ID3D11SamplerState[layout.SamplerCount];
	}

	ID3D11BufferHandle[] ConstantBufferHandles;
	ID3D11Buffer[] ConstantBuffers;
	int[] ConstantBuffersOffsets;
	int[] ConstantBuffersSizes;
	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding, uint offset = 0 ) where T : unmanaged {
		DebugMemoryAlignment.AssertStructAlignment( this, binding, typeof( T ) );
		binding = layout.BindingLookup[binding];

		ConstantBufferHandles[binding] = (ID3D11BufferHandle)buffer;
		ConstantBuffersOffsets[binding] = (int)(offset * IBuffer<T>.AlignedStride(256)) / 16;
		ConstantBuffersSizes[binding] = (int)IBuffer<T>.Stride;
	}

	ID3D11ShaderResourceView[] SamplerResources;
	ID3D11SamplerState[] SamplerStates;
	public void SetSampler ( ITexture2D texture, uint binding ) {
		var tx = (Texture2D)texture;
		binding = layout.BindingLookup[binding];

		SamplerResources[binding] = tx.ResourceView;
		SamplerStates[binding] = tx.Sampler;
	}

	public void Apply ( ID3D11DeviceContext ctx ) {
		var context = (ID3D11DeviceContext1)ctx;

		for ( int i = 0; i < ConstantBufferHandles.Length; i++ ) {
			ConstantBuffers[i] = ConstantBufferHandles[i].Handle!; // TODO maybe can be removed if we can reallocate withotu changing the pointer
			// TODO also we need to remove some layers from this api bc some calls are doing unnecessary operations (pretty much everything that uses the C# classes actually)
			// and also not allowing us to pass pointers which is yucky
		}

		// TODO set only in shaders that need it..?
		context.VSSetConstantBuffers1( layout.FirstConstantBuffer, layout.ConstantBufferCount, ConstantBuffers, ConstantBuffersOffsets, ConstantBuffersSizes );
		context.PSSetConstantBuffers1( layout.FirstConstantBuffer, layout.ConstantBufferCount, ConstantBuffers, ConstantBuffersOffsets, ConstantBuffersSizes );

		context.PSSetShaderResources( layout.FirstSampler, SamplerResources );
		context.PSSetSamplers( layout.FirstSampler, SamplerStates );
	}

	protected override void Dispose ( bool disposing ) {
		DebugMemoryAlignment.ClearDebugData( this );
	}
}
