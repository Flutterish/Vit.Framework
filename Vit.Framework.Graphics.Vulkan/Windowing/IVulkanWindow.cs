using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Windowing;

public interface IVulkanWindow {
	VkSurfaceKHR GetSurface ( VulkanInstance vulkan );
}