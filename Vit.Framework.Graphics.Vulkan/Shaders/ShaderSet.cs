using System.Collections.Immutable;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Vulkan.Uniforms;
using Vit.Framework.Memory;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Shaders;

public class ShaderSet : DisposableObject, IShaderSet {
	public readonly ImmutableArray<ShaderModule> Modules;
	public IEnumerable<IShaderPart> Parts => Modules;

	public unsafe ShaderSet ( IEnumerable<ShaderModule> modules, VertexInputDescription? vertexInput ) {
		Modules = modules.ToImmutableArray();
		var uniformInfo = this.CreateUniformInfo();

		if ( vertexInput != null ) {
			(Attributes, AttributeSets) = vertexInput.GenerateVertexBindings();
			VertexBufferCount = vertexInput.BufferBindings.Any() ? (int)vertexInput.BufferBindings.Max( x => x.Key ) + 1 : 0;
		}
		else {
			Attributes = Array.Empty<VkVertexInputAttributeDescription>();
			AttributeSets = Array.Empty<VkVertexInputBindingDescription>();
		}

		var setCount = this.GetUniformSetIndices().Count();
		UniformSets = new IDescriptorSet?[setCount];
		DescriptorSets = new VkDescriptorSet[setCount];
		DescriptorSetLayouts = new DescriptorSetLayout[setCount];

		for ( uint i = 0; i < setCount; i++ ) {
			DescriptorSetLayouts[i] = new( Modules[0].Device, uniformInfo.Sets.GetValueOrDefault( i ) ?? new() );
		}
	}

	public DescriptorSetLayout[] DescriptorSetLayouts;
	public VkDescriptorSet[] DescriptorSets;
	public IDescriptorSet?[] UniformSets;

	public int VertexBufferCount;
	public VkVertexInputAttributeDescription[] Attributes;
	public VkVertexInputBindingDescription[] AttributeSets;

	public IUniformSetPool CreateUniformSetPool ( uint set, uint size ) {
		return new DescriptorPool( DescriptorSetLayouts[set], size );
	}

	public void SetUniformSet ( IUniformSet uniforms, uint set = 0 ) {
		var value = (IDescriptorSet)uniforms;
		UniformSets[set] = value;
		DescriptorSets[set] = value.Handle;
	}

	protected override unsafe void Dispose ( bool disposing ) {
		foreach ( var i in DescriptorSetLayouts ) {
			i.Dispose();
		}
	}
}
