using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Specialisation;
using Vit.Framework.Graphics.Rendering.Textures;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan;

public class VulkanApi : GraphicsApi {
	public static readonly GraphicsApiType GraphicsApiType = new() {
		KnownName = KnownGraphicsApiName.Vulkan,
		Name = "Vulkan",
		Version = -1
	};

	public readonly VulkanInstance Instance;

	public VulkanApi ( VulkanInstance instance, IEnumerable<RenderingCapabilities> capabilities ) : base( GraphicsApiType, capabilities ) {
		Instance = instance;
	}

	protected override void Dispose ( bool disposing ) {
		Instance.Dispose();
	}

	public static Dictionary<PixelFormat, VkFormat> formats = new() {
		[PixelFormat.Rgba8] = VkFormat.R8g8b8a8Unorm,
		[PixelFormat.D24] = VkFormat.X8D24UnormPack32,
		[PixelFormat.D32f] = VkFormat.D32Sfloat,
		[PixelFormat.D24S8ui] = VkFormat.D24UnormS8Uint,
		[PixelFormat.D32fS8ui] = VkFormat.D32SfloatS8Uint,
		[PixelFormat.S8ui] = VkFormat.S8Uint
	};
}
