using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Textures;

public static class VkFormatExtensions {
	public static bool HasStencilAttachment ( this VkFormat format ) => format switch {
		VkFormat.S8Uint => true,
		VkFormat.D16UnormS8Uint => true,
		VkFormat.D24UnormS8Uint => true,
		VkFormat.D32SfloatS8Uint => true,
		_ => false
	};
}
