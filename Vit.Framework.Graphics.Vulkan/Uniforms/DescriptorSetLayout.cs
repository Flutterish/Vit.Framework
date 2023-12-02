using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Vulkan.Shaders;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Uniforms;

public class DescriptorSetLayout : DisposableVulkanObject<VkDescriptorSetLayout> {
	public readonly Device Device;
	public readonly VkDescriptorSetLayoutBinding[] LayoutBindings;
	public readonly UniformSetInfo Type;
	public unsafe DescriptorSetLayout ( Device device, UniformSetInfo type ) {
		Type = type;
		Device = device;
		LayoutBindings = type.GenerateUniformBindingsSet();

		fixed ( VkDescriptorSetLayoutBinding* LayoutBindingsPtr = LayoutBindings ) {
			var uniformInfo = new VkDescriptorSetLayoutCreateInfo() {
				sType = VkStructureType.DescriptorSetLayoutCreateInfo,
				bindingCount = (uint)LayoutBindings.Length,
				pBindings = LayoutBindingsPtr
			};
			Vk.vkCreateDescriptorSetLayout( device, &uniformInfo, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
		}
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyDescriptorSetLayout( Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
