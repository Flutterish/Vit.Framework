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
	}

	// TODO binding and offset values of these should be generated based on some logical linking between mesh vertex buffers and material attributes
	public VkVertexInputAttributeDescription[] Attributes;
	public VkVertexInputBindingDescription[] AttributeSets;

	Dictionary<uint, UniformSet> uniformSets = new();
	public IUniformSet GetUniformSet ( uint set = 0 ) {
		if ( !uniformSets.TryGetValue( set, out var value ) )
			uniformSets.Add( set, value = new( Modules[0].Device, this.CreateUniformSetInfo( set ) ) );

		return value;
	}

	protected override unsafe void Dispose ( bool disposing ) {
		foreach ( var (_, set) in uniformSets ) {
			set.Dispose();
		}
	}
}
