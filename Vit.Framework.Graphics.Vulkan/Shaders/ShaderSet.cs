using System.Collections.Immutable;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Uniforms;
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

		foreach ( var set in this.GetUniformSetIndices() ) {
			UniformSets.Add( new( Modules[0].Device, this.CreateUniformSetInfo( set ) ) );
		}
		DescriptorSets = new VkDescriptorSet[UniformSets.Count];
		defaultsCreated = new bool[UniformSets.Count];
		for ( int i = 0; i < DescriptorSets.Length; i++ ) {
			DescriptorSets[i] = UniformSets[i].DescriptorSet;
		}
	}

	// TODO binding and offset values of these should be generated based on some logical linking between mesh vertex buffers and material attributes
	public VkVertexInputAttributeDescription[] Attributes;
	public VkVertexInputBindingDescription[] AttributeSets;

	public List<UniformSet> UniformSets = new();
	public VkDescriptorSet[] DescriptorSets;
	public IUniformSet GetUniformSet ( uint set = 0 ) {
		defaultsCreated[set] = true;
		return UniformSets[(int)set];
	}

	bool[] defaultsCreated;
	public IUniformSet CreateUniformSet ( uint set = 0 ) {
		if ( defaultsCreated[set] ) {
			return new UniformSet( UniformSets[(int)set] );
		}
		else {
			defaultsCreated[set] = true;
			return UniformSets[(int)set];
		}
	}

	public void SetUniformSet ( IUniformSet uniforms, uint set = 0 ) {
		UniformSets[(int)set] = (UniformSet)uniforms;
		defaultsCreated[set] = true;
		DescriptorSets[set] = ((UniformSet)uniforms).DescriptorSet;
	}

	protected override unsafe void Dispose ( bool disposing ) {
		foreach ( var set in UniformSets ) {
			set.Dispose();
		}
	}
}
