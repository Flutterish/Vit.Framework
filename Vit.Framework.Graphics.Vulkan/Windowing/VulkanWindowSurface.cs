using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Vulkan.Queues;
using Vit.Framework.Interop;
using Vit.Framework.Windowing;

namespace Vit.Framework.Graphics.Vulkan.Windowing;

public class VulkanWindowSurface : WindowGraphicsSurface {
	IVulkanWindow window;
	WindowSurfaceArgs args;
	public VulkanWindowSurface ( VulkanApi graphicsApi, WindowSurfaceArgs args, IVulkanWindow window ) : base( graphicsApi, args ) {
		this.window = window;
		this.args = args;
	}

	protected override (ISwapchain swapchain, IRenderer renderer) CreateSwapchain () {
		var vulkan = (VulkanApi)GraphicsApi;

		var surface = window.GetSurface( vulkan.Instance );
		var (physicalDevice, swapchainInfo) = vulkan.Instance.GetBestDeviceInfo( surface );

		var device = physicalDevice.CreateDevice( SwapchainInfo.RequiredExtensionsCstr, new CString[] { }, new[] {
			swapchainInfo.GraphicsFamily,
			swapchainInfo.PresentFamily
		} );

		var swapchain = device.CreateSwapchain( surface, swapchainInfo.SelectBest(), window.PixelSize );
		var graphicsQueue = device.GetQueue( swapchainInfo.GraphicsFamily );
		var presentQueue = device.GetQueue( swapchainInfo.PresentFamily );
		var renderer = new VulkanRenderer( vulkan, device, graphicsQueue );
		return (new WindowSwapchain( window, swapchain, presentQueue, renderer, args ), renderer);
	}
}
