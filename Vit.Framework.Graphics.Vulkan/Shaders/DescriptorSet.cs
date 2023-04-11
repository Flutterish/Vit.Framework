using Vit.Framework.Graphics.Vulkan.Buffers;
using Vit.Framework.Graphics.Vulkan.Textures;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Shaders;

public class DescriptorSet : VulkanObject<VkDescriptorSet> {
	public unsafe DescriptorSet ( DescriptorPool pool, VkDescriptorSetLayout layout ) {
		var info = new VkDescriptorSetAllocateInfo() {
			sType = VkStructureType.DescriptorSetAllocateInfo,
			descriptorPool = pool,
			descriptorSetCount = 1,
			pSetLayouts = &layout
		};

		Vk.vkAllocateDescriptorSets( pool.Device, &info, out Instance ).Validate();
	}

	public unsafe void ConfigureUniforms<T> ( Buffer<T> uniformBuffer, uint binding, ulong offset = 0 ) where T : unmanaged {
		var bufferInfo = new VkDescriptorBufferInfo() {
			buffer = uniformBuffer,
			offset = offset,
			range = Buffer<T>.Stride
		};

		var write = new VkWriteDescriptorSet() {
			sType = VkStructureType.WriteDescriptorSet,
			dstSet = this,
			dstBinding = binding,
			dstArrayElement = 0,
			descriptorType = VkDescriptorType.UniformBuffer,
			descriptorCount = 1,
			pBufferInfo = &bufferInfo
		};

		Vk.vkUpdateDescriptorSets( uniformBuffer.Device, 1, &write, 0, 0 );
	}

	public unsafe void ConfigureTexture ( Image image, VkSampler sampler, uint binding ) {
		var imageInfo = new VkDescriptorImageInfo() {
			imageLayout = VkImageLayout.ShaderReadOnlyOptimal,
			imageView = image.View,
			sampler = sampler
		};

		var write = new VkWriteDescriptorSet() {
			sType = VkStructureType.WriteDescriptorSet,
			dstSet = this,
			dstBinding = binding,
			dstArrayElement = 0,
			descriptorType = VkDescriptorType.CombinedImageSampler,
			descriptorCount = 1,
			pImageInfo = &imageInfo
		};

		Vk.vkUpdateDescriptorSets( image.Device, 1, &write, 0, 0 );
	}
}
