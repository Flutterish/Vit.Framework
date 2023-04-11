using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Textures;

public class Sampler : DisposableVulkanObject<VkSampler> {
	public readonly Device Device;
	public unsafe Sampler ( Device device ) {
		var info = new VkSamplerCreateInfo() {
			sType = VkStructureType.SamplerCreateInfo,
			magFilter = VkFilter.Linear,
			minFilter = VkFilter.Linear,
			addressModeU = VkSamplerAddressMode.Repeat,
			addressModeV = VkSamplerAddressMode.Repeat,
			addressModeW = VkSamplerAddressMode.Repeat,
			anisotropyEnable = true,
			maxAnisotropy = device.PhysicalDevice.Properties.limits.maxSamplerAnisotropy,
			borderColor = VkBorderColor.IntOpaqueBlack,
			unnormalizedCoordinates = false,
			compareEnable = false,
			compareOp = VkCompareOp.Always,
			mipmapMode = VkSamplerMipmapMode.Linear,
			mipLodBias = 0,
			minLod = 0,
			maxLod = 0
		};

		Vk.vkCreateSampler( device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroySampler( Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
