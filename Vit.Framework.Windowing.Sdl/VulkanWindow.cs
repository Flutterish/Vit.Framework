using SDL2;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Graphics.Vulkan.Queues;
using Vit.Framework.Graphics.Vulkan.Windowing;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vulkan;
using NativeSwapchain = Vit.Framework.Graphics.Rendering.Queues.NativeSwapchain;

namespace Vit.Framework.Windowing.Sdl;

class VulkanWindow : SdlWindow, IVulkanWindow { // TODO remove the IVulkanWindow
	public VulkanWindow ( SdlHost host ) : base( host, GraphicsApiType.Vulkan ) { }

	public override Size2<int> PixelSize {
		get {
			SDL.SDL_Vulkan_GetDrawableSize( Pointer, out int pixelWidth, out int pixelHeight );
			return new( pixelWidth, pixelHeight );
		}
	}

	public override (NativeSwapchain swapchain, Renderer renderer) CreateSwapchain ( GraphicsApi api, SwapChainArgs args ) {
		if ( api is not VulkanApi vulkan )
			throw new ArgumentException( "Graphics API must be a Vulkan API created from the same host as this window", nameof(api) );

		var surface = GetSurface( vulkan.Instance );
		var (physicalDevice, swapchainInfo) = vulkan.Instance.GetBestDeviceInfo( surface );

		var device = physicalDevice.CreateDevice( SwapchainInfo.RequiredExtensionsCstr, new CString[] { }, new[] {
			swapchainInfo.GraphicsFamily,
			swapchainInfo.PresentFamily
		} );
		
		var swapchain = device.CreateSwapchain( surface, swapchainInfo.SelectBest(), PixelSize );
		var graphicsQueue = device.GetQueue( swapchainInfo.GraphicsFamily );
		var presentQueue = device.GetQueue( swapchainInfo.PresentFamily );
		var renderer = new VulkanRenderer( vulkan, device, graphicsQueue );
		return (new WindowSwapchain(this, swapchain, presentQueue, renderer, args), renderer);
	}

	public VkSurfaceKHR GetSurface ( VulkanInstance vulkan ) {
		SDL.SDL_Vulkan_CreateSurface( Pointer, vulkan.Handle.Handle, out var surface );
		return new VkSurfaceKHR( (ulong)surface );
	}
}
