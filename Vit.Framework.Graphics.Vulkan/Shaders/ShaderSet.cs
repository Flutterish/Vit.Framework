using System.Collections.Immutable;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;
using Vit.Framework.Graphics.Vulkan.Uniforms;
using Vit.Framework.Memory;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Shaders;

public class ShaderSet : DisposableObject, IShaderSet {
	public readonly ImmutableArray<ShaderModule> Modules;
	public IEnumerable<IShaderPart> Parts => Modules;

	public ShaderSet ( params ShaderModule[] modules ) : this( (IEnumerable<ShaderModule>)modules ) { }

	public unsafe ShaderSet ( IEnumerable<ShaderModule> modules ) {
		Modules = modules.ToImmutableArray();
		if ( modules.FirstOrDefault( x => x.StageCreateInfo.stage.HasFlag( VkShaderStageFlags.Vertex ) ) is ShaderModule vertexModule ) {
			(Attributes, AttributeSets) = vertexModule.Spirv.Reflections.GenerateVertexBindings();
		}
		else {
			Attributes = Array.Empty<VkVertexInputAttributeDescription>();
			AttributeSets = Array.Empty<VkVertexInputBindingDescription>();
		}

		var setCount = this.GetUniformSetIndices().Count();
		UniformSets = new UniformSet?[setCount];
		DescriptorSets = new VkDescriptorSet[setCount];
	}

	// TODO binding and offset values of these should be generated based on some logical linking between mesh vertex buffers and material attributes
	public VkVertexInputAttributeDescription[] Attributes;
	public VkVertexInputBindingDescription[] AttributeSets;

	public UniformSet?[] UniformSets;
	public VkDescriptorSet[] DescriptorSets;
	public IUniformSet? GetUniformSet ( uint set = 0 ) {
		return UniformSets[set];
	}

	public IUniformSet CreateUniformSet ( uint set = 0 ) {
		if ( UniformSets[set] is UniformSet existing ) {
			var value = new UniformSet( existing );
			DebugMemoryAlignment.SetDebugData( value, set, this );
			return value;
		}
		else {
			var value = new UniformSet( Modules[0].Device, this.CreateUniformSetInfo( set ) );
			DebugMemoryAlignment.SetDebugData( value, set, this );
			return value;
		}
	}

	public void SetUniformSet ( IUniformSet uniforms, uint set = 0 ) {
		UniformSets[set] = (UniformSet)uniforms;
		DescriptorSets[set] = ((UniformSet)uniforms).DescriptorSet;
	}

	protected override unsafe void Dispose ( bool disposing ) { }
}
