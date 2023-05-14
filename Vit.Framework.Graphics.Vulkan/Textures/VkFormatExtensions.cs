using Vit.Framework.Graphics.Rendering.Textures;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Textures;

public static class VkFormatExtensions {
	public static DepthFormat GetDepthFormat ( this VkFormat format ) => format switch {
		VkFormat.D16Unorm => DepthFormat.Bits16,
		VkFormat.D16UnormS8Uint => DepthFormat.Bits16,
		VkFormat.D24UnormS8Uint => DepthFormat.Bits24,
		VkFormat.D32Sfloat => DepthFormat.Bits32,
		VkFormat.D32SfloatS8Uint => DepthFormat.Bits32,
		_ => DepthFormat.None
	};

	public static StencilFormat GetStencilFormat ( this VkFormat format ) => format switch {
		VkFormat.D16UnormS8Uint => StencilFormat.Bits8,
		VkFormat.D24UnormS8Uint => StencilFormat.Bits8,
		VkFormat.D32SfloatS8Uint => StencilFormat.Bits8,
		VkFormat.S8Uint => StencilFormat.Bits8,
		_ => StencilFormat.None
	};

	public static VkFormat GetFormat ( this (DepthFormat depth, StencilFormat stencil) type ) => type switch {
		(DepthFormat.Bits16, StencilFormat.None) => VkFormat.D16Unorm,
		(DepthFormat.Bits16, StencilFormat.Bits8) => VkFormat.D16UnormS8Uint,
		(DepthFormat.Bits24, StencilFormat.Bits8) => VkFormat.D24UnormS8Uint,
 		(DepthFormat.Bits32, StencilFormat.None) => VkFormat.D32Sfloat,
		(DepthFormat.Bits32, StencilFormat.Bits8) => VkFormat.D32SfloatS8Uint,
		(DepthFormat.None, StencilFormat.Bits8) => VkFormat.S8Uint,
		_ => VkFormat.Undefined
	};
}
