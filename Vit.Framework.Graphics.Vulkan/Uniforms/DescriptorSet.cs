using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;
using Vit.Framework.Graphics.Vulkan.Buffers;
using Vit.Framework.Graphics.Vulkan.Shaders;
using Vit.Framework.Graphics.Vulkan.Textures;
using Vit.Framework.Interop;
using Vit.Framework.Memory;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Uniforms;

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

	public unsafe void ConfigureUniforms<T> ( Buffer<T> uniformBuffer, uint binding, uint offset = 0 ) where T : unmanaged {
		var bufferInfo = new VkDescriptorBufferInfo() {
			buffer = uniformBuffer,
			offset = offset * Buffer<T>.Stride,
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

public class UniformSet : DisposableObject, IUniformSet {
	public readonly VkDescriptorSetLayoutBinding[] LayoutBindings;
	public readonly VkDescriptorSetLayout Layout;
	public readonly DescriptorPool DescriptorPool;
	public readonly DescriptorSet DescriptorSet;
	Ref<uint> layoutReferenceCount;

	public unsafe UniformSet ( Device device, UniformSetInfo info ) {
		layoutReferenceCount = new( 1 );
		LayoutBindings = info.GenerateUniformBindingsSet();
		var uniformInfo = new VkDescriptorSetLayoutCreateInfo() {
			sType = VkStructureType.DescriptorSetLayoutCreateInfo,
			bindingCount = (uint)LayoutBindings.Length,
			pBindings = LayoutBindings.Data()
		};
		Vk.vkCreateDescriptorSetLayout( device, &uniformInfo, VulkanExtensions.TODO_Allocator, out Layout ).Validate();
		DescriptorPool = LayoutBindings.CreateDescriptorPool( device );
		DescriptorSet = DescriptorPool.CreateSet( Layout );
	}

	public UniformSet ( UniformSet value ) {
		layoutReferenceCount = value.layoutReferenceCount;
		layoutReferenceCount.Value++;
		var device = value.DescriptorPool.Device;
		LayoutBindings = value.LayoutBindings;
		Layout = value.Layout;
		DescriptorPool = LayoutBindings.CreateDescriptorPool( device );
		DescriptorSet = DescriptorPool.CreateSet( Layout );
	}

	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding, uint offset = 0 ) where T : unmanaged {
		DebugMemoryAlignment.AssertStructAlignment( this, binding, typeof( T ) );
		DescriptorSet.ConfigureUniforms( (Buffer<T>)buffer, binding, offset );
	}

	public void SetSampler ( ITexture texture, uint binding ) {
		var image = (ImageTexture)texture;
		DescriptorSet.ConfigureTexture( image.Image, image.Sampler, binding );
	}

	protected override unsafe void Dispose ( bool disposing ) {
		DebugMemoryAlignment.ClearDebugData( this );
		DescriptorPool.Dispose();

		layoutReferenceCount.Value--;
		if ( layoutReferenceCount.Value > 0 )
			return;

		Vk.vkDestroyDescriptorSetLayout( DescriptorPool.Device, Layout, VulkanExtensions.TODO_Allocator );
	}
}
