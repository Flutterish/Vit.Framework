using Vit.Framework.Graphics.Rendering.Textures;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Textures;

public class Sampler : DisposableVulkanObject<VkSampler>, ISampler {
	const VkBorderColor VK_BORDER_COLOR_FLOAT_CUSTOM_EXT = (VkBorderColor)1000287003;
	const VkStructureType VK_STRUCTURE_TYPE_SAMPLER_CUSTOM_BORDER_COLOR_CREATE_INFO_EXT = (VkStructureType)1000287000;

	public readonly Device Device;
	public unsafe Sampler ( Device device, SamplerDescription description ) {
		Device = device;

		static VkFilter filter ( FilteringMode mode ) {
			return mode switch {
				FilteringMode.Nearest => VkFilter.Nearest,
				FilteringMode.Linear or _ => VkFilter.Linear
			};
		}

		static VkSamplerMipmapMode mipmapFilter ( MipmapMode mode ) {
			return mode switch {
				MipmapMode.None => VkSamplerMipmapMode.Nearest,
				MipmapMode.Nearest => VkSamplerMipmapMode.Nearest,
				MipmapMode.Linear or _ => VkSamplerMipmapMode.Linear
			};
		}

		static VkSamplerAddressMode wrapMode ( TextureWrapMode mode ) {
			return mode switch {
				TextureWrapMode.Repeat => VkSamplerAddressMode.Repeat,
				TextureWrapMode.MirroredRepeat => VkSamplerAddressMode.MirroredRepeat,
				TextureWrapMode.ClampToEdge => VkSamplerAddressMode.ClampToEdge,
				TextureWrapMode.TransparentBlackBorder or _ => VkSamplerAddressMode.ClampToBorder
			};
		}

		var info = new VkSamplerCreateInfo() {
			sType = VkStructureType.SamplerCreateInfo,
			magFilter = filter( description.MagnificationFilter ),
			minFilter = filter( description.MinificationFilter ),
			addressModeU = wrapMode( description.WrapU ),
			addressModeV = wrapMode( description.WrapV ),
			anisotropyEnable = description.EnableAnisotropy,
			maxAnisotropy = description.MaximumAnisotropicFiltering,
			mipmapMode = mipmapFilter( description.MipmapMode ),
			borderColor = VkBorderColor.FloatTransparentBlack,
			mipLodBias = description.MipmapLevelBias,
			minLod = description.MinimimMipmapLevel,
			maxLod = description.MaximimMipmapLevel
		};

		Vk.vkCreateSampler( device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroySampler( Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}

public unsafe struct VkSamplerCustomBorderColorCreateInfoEXT {
	public VkStructureType sType;
	public void* pNext;
	public VkClearColorValue customBorderColor;
	public VkFormat format;
}