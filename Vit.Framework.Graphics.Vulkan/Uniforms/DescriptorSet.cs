using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;
using Vit.Framework.Graphics.Vulkan.Textures;
using Vit.Framework.Memory;
using Vulkan;
using Buffer = Vit.Framework.Graphics.Vulkan.Buffers.Buffer;

namespace Vit.Framework.Graphics.Vulkan.Uniforms;

public interface IDescriptorSet : IUniformSet, IVulkanHandle<VkDescriptorSet> {
	VkDescriptorSetLayout Layout { get; }
}

public class DescriptorSet : VulkanObject<VkDescriptorSet>, IDescriptorSet {
	public VkDescriptorSetLayout Layout { get; }
	public unsafe DescriptorSet ( DescriptorPool pool, VkDescriptorSetLayout layout ) {
		Layout = layout;
		var info = new VkDescriptorSetAllocateInfo() {
			sType = VkStructureType.DescriptorSetAllocateInfo,
			descriptorPool = pool,
			descriptorSetCount = 1,
			pSetLayouts = &layout
		};

		Vk.vkAllocateDescriptorSets( pool.Device, &info, out Instance ).Validate();
	}

	public unsafe void SetUniformBufferRaw ( IBuffer buffer, uint binding, uint size, uint offset = 0 ) {
		var uniformBuffer = (Buffer)buffer;

		var bufferInfo = new VkDescriptorBufferInfo() {
			buffer = uniformBuffer,
			offset = offset,
			range = size
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

	public unsafe void SetStorageBufferRaw ( IBuffer _buffer, uint binding, uint size, uint offset = 0 ) {
		var buffer = (Buffer)_buffer;
		var bufferInfo = new VkDescriptorBufferInfo {
			buffer = buffer,
			offset = offset,
			range = size
		};

		var write = new VkWriteDescriptorSet() {
			sType = VkStructureType.WriteDescriptorSet,
			dstSet = this,
			dstBinding = binding,
			dstArrayElement = 0,
			descriptorType = VkDescriptorType.StorageBuffer,
			descriptorCount = 1,
			pBufferInfo = &bufferInfo
		};

		Vk.vkUpdateDescriptorSets( buffer.Device, 1, &write, 0, 0 );
	}

	public unsafe void SetSampler ( ITexture2DView texture, ISampler _sampler, uint binding ) {
		var imageTexture = (ImageView)texture;
		VkSampler sampler = ((Sampler)_sampler).Handle;

		var imageInfo = new VkDescriptorImageInfo() {
			imageLayout = VkImageLayout.ShaderReadOnlyOptimal,
			imageView = imageTexture.Handle,
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

		Vk.vkUpdateDescriptorSets( imageTexture.Device, 1, &write, 0, 0 );
	}
}
