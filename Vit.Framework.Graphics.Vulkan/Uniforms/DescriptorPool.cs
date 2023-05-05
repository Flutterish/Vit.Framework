using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Uniforms;

public class DescriptorPool : DisposableVulkanObject<VkDescriptorPool> {
	public readonly VkDevice Device;
	public unsafe DescriptorPool ( VkDevice device, params VkDescriptorPoolSize[] values ) {
		Device = device;

		var info = new VkDescriptorPoolCreateInfo() {
			sType = VkStructureType.DescriptorPoolCreateInfo,
			poolSizeCount = (uint)values.Length,
			pPoolSizes = values.Data(),
			maxSets = 1
		};

		Vk.vkCreateDescriptorPool( device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
	}

	public DescriptorSet CreateSet ( VkDescriptorSetLayout layout ) {
		return new( this, layout );
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyDescriptorPool( Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
