using System.Collections.Immutable;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Vulkan.Buffers;
using Vit.Framework.Interop;
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

		var device = Modules[0].Device;
		var layouts = Modules.Select( x => x.Spirv.Reflections ).GenerateUniformBindingsSet( 0 );
		var uniformInfo = new VkDescriptorSetLayoutCreateInfo() {
			sType = VkStructureType.DescriptorSetLayoutCreateInfo,
			bindingCount = (uint)layouts.Length,
			pBindings = layouts.Data()
		};
		Vk.vkCreateDescriptorSetLayout( device, &uniformInfo, VulkanExtensions.TODO_Allocator, out Uniforms ).Validate();
		DescriptorPool = layouts.CreateDescriptorPool( device );
		DescriptorSet = DescriptorPool.CreateSet( Uniforms );
	}

	// TODO binding and offset values of these should be generated based on some logical linking between mesh vertex buffers and material attributes
	public VkVertexInputAttributeDescription[] Attributes;
	public VkVertexInputBindingDescription[] AttributeSets;

	// TODO these are only for set = 0 and are not unique per shader set
	public readonly VkDescriptorSetLayout Uniforms;
	public readonly DescriptorPool DescriptorPool;
	public readonly DescriptorSet DescriptorSet;

	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding = 0, uint offset = 0 ) where T : unmanaged {
		DescriptorSet.ConfigureUniforms( (Buffer<T>)buffer, binding, offset );
	}

	protected override unsafe void Dispose ( bool disposing ) {
		DescriptorPool.Dispose();
		Vk.vkDestroyDescriptorSetLayout( Modules[0].Device, Uniforms, VulkanExtensions.TODO_Allocator );
	}
}
