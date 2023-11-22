using System.Diagnostics;
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

		ConstantBuffers = new ID3D11Buffer[layout.ConstantBufferCount];
		ConstantBuffersOffsets = new int[layout.ConstantBufferCount];
		ConstantBuffersSizes = new int[layout.ConstantBufferCount];

		SamplerResources = new ID3D11ShaderResourceView[layout.SamplerCount];
		SamplerStates = new ID3D11SamplerState[layout.SamplerCount];

		StorageBufferResources = new ID3D11ShaderResourceView[layout.StorageBufferCount];
	}

	ID3D11Buffer[] ConstantBuffers;
	int[] ConstantBuffersOffsets;
	int[] ConstantBuffersSizes;
	public void SetUniformBufferRaw ( IBuffer buffer, uint binding, uint size, uint offset = 0 ) {
		binding = layout.BindingLookup[binding];

		ConstantBuffers[binding] = ((ID3D11BufferHandle)buffer).Handle;
		ConstantBuffersOffsets[binding] = (int)(offset / 16);
		ConstantBuffersSizes[binding] = ((int)size + 15) / 16 * 16;
	}

	ID3D11ShaderResourceView[] StorageBufferResources;
	public void SetStorageBufferRaw ( IBuffer buffer, uint binding, uint size, uint offset = 0 ) {
		var buf = (ID3D11BufferHandle)buffer;
		binding = layout.BindingLookup[binding];

		Debug.Assert( offset == 0 );
		StorageBufferResources[binding] = buf.ResourceView!;
	}

	ID3D11ShaderResourceView[] SamplerResources;
	ID3D11SamplerState[] SamplerStates;
	public void SetSampler ( ITexture2DView texture, ISampler sampler, uint binding ) {
		binding = layout.BindingLookup[binding];

		SamplerResources[binding] = ((Texture2DView)texture).ResourceView;
		SamplerStates[binding] = ((SamplerState)sampler).Sampler;
	}

	public void Apply ( ID3D11DeviceContext ctx ) {
		var context = (ID3D11DeviceContext1)ctx;

		// TODO also we need to remove some layers from this api bc some calls are doing unnecessary operations (pretty much everything that uses the C# classes actually)
		// and also not allowing us to pass pointers which is yucky

		// TODO set only in shaders that need it..?
		context.VSSetConstantBuffers1( layout.FirstConstantBuffer, layout.ConstantBufferCount, ConstantBuffers, ConstantBuffersOffsets, ConstantBuffersSizes );
		context.PSSetConstantBuffers1( layout.FirstConstantBuffer, layout.ConstantBufferCount, ConstantBuffers, ConstantBuffersOffsets, ConstantBuffersSizes );

		context.PSSetShaderResources( layout.FirstSampler, SamplerResources );
		context.PSSetSamplers( layout.FirstSampler, SamplerStates );

		context.PSSetShaderResources( layout.FirstStorageBuffer, layout.StorageBufferCount, StorageBufferResources );
	}

	protected override void Dispose ( bool disposing ) {
		DebugMemoryAlignment.ClearDebugData( this );
	}
}
