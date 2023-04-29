using Vit.Framework.Windowing;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Windowing;

public interface IVulkanWindow : IWindow {
	VkSurfaceKHR GetSurface ( VulkanInstance vulkan );
}