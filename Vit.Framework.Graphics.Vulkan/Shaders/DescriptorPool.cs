using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Shaders;

public class DescriptorPool : DisposableVulkanObject<VkDescriptorPool> {
	public readonly VkDevice Device;
	public unsafe DescriptorPool ( VkDevice device, VkDescriptorType type, uint count ) {
		Device = device;

		var size = new VkDescriptorPoolSize() {
			type = type,
			descriptorCount = count
		};

		var info = new VkDescriptorPoolCreateInfo() {
			sType = VkStructureType.DescriptorPoolCreateInfo,
			poolSizeCount = 1,
			pPoolSizes = &size,
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
