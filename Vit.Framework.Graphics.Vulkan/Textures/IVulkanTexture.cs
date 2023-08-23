using Vit.Framework.Graphics.Rendering.Textures;

namespace Vit.Framework.Graphics.Vulkan.Textures;

public interface IVulkanTexture : ITexture2D {
	VulkanTextureType Type { get; }
}

public enum VulkanTextureType {
	Image,
	Buffer
}