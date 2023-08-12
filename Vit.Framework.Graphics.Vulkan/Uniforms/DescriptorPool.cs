using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;
using Vit.Framework.Graphics.Vulkan.Shaders;
using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Uniforms;

public class DescriptorPool : DisposableVulkanObject<VkDescriptorPool>, IUniformSetPool {
	public readonly Device Device;
	public readonly UniformSetInfo Type;
	public readonly VkDescriptorSetLayout Layout;
	public unsafe DescriptorPool ( Device device, uint size, UniformSetInfo type ) {
		Device = device;
		Type = type;

		var layoutBindings = type.GenerateUniformBindingsSet();

		var values = new VkDescriptorPoolSize[layoutBindings.Length];
		for ( int i = 0; i < values.Length; i++ ) {
			values[i] = new() {
				type = layoutBindings[i].descriptorType,
				descriptorCount = 1
			};
		}

		var info = new VkDescriptorPoolCreateInfo() {
			sType = VkStructureType.DescriptorPoolCreateInfo,
			poolSizeCount = (uint)values.Length,
			pPoolSizes = values.Data(),
			maxSets = size
		};
		Vk.vkCreateDescriptorPool( device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();

		var uniformInfo = new VkDescriptorSetLayoutCreateInfo() {
			sType = VkStructureType.DescriptorSetLayoutCreateInfo,
			bindingCount = (uint)layoutBindings.Length,
			pBindings = layoutBindings.Data()
		};
		Vk.vkCreateDescriptorSetLayout( device, &uniformInfo, VulkanExtensions.TODO_Allocator, out Layout ).Validate();

		created = new( (int)size );
	}

	public DescriptorSet CreateSet () {
		var value = new DescriptorSet( this, Layout );
		DebugMemoryAlignment.SetDebugData( value, Type.Resources );
		return value;
	}

	Stack<IUniformSet> created;
	public IUniformSet Rent () {
		if ( !created.TryPop( out var set ) )
			set = CreateSet();

		return set;
	}

	public void Free ( IUniformSet set ) {
		created.Push( set );
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyDescriptorSetLayout( Device, Layout, VulkanExtensions.TODO_Allocator );
		Vk.vkDestroyDescriptorPool( Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
