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

	public unsafe void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding, uint offset = 0 ) where T : unmanaged {
		DebugMemoryAlignment.AssertStructAlignment( this, binding, typeof( T ) );
		var uniformBuffer = (Buffer)buffer;

		var bufferInfo = new VkDescriptorBufferInfo() {
			buffer = uniformBuffer,
			offset = offset * IBuffer<T>.AlignedStride(256),
			range = IBuffer<T>.Stride
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

	public unsafe void SetSampler ( ITexture2D texture, uint binding ) {
		var imageTexture = (ImageTexture)texture;
		Image image = imageTexture.Image;
		VkSampler sampler = imageTexture.Sampler;

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

	public void Dispose () {
		DebugMemoryAlignment.ClearDebugData( this );
	}

	public void SetSampler ( ITexture2DView texture, ISampler sampler, uint binding ) {
		throw new NotImplementedException();
	}
}

public class StandaloneUniformSet : DisposableObject, IDescriptorSet {
	public readonly DescriptorPool DescriptorPool;
	public readonly DescriptorSet DescriptorSet;
	public VkDescriptorSetLayout Layout => DescriptorSet.Layout;
	public VkDescriptorSet Handle => DescriptorSet.Handle;

	public unsafe StandaloneUniformSet ( DescriptorSetLayout layout ) {
		DescriptorPool = new DescriptorPool( layout, 1 );
		DescriptorSet = DescriptorPool.CreateSet();
	}

	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding, uint offset = 0 ) where T : unmanaged {
		DescriptorSet.SetUniformBuffer( buffer, binding, offset );
	}

	public void SetSampler ( ITexture2DView texture, ISampler sampler, uint binding ) {
		DescriptorSet.SetSampler( texture, sampler, binding );
	}

	protected override unsafe void Dispose ( bool disposing ) {
		DescriptorSet.Dispose();
		DescriptorPool.Dispose();
	}
}
