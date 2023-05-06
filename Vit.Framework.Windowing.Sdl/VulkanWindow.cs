using SDL2;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Graphics.Vulkan.Queues;
using Vit.Framework.Graphics.Vulkan.Windowing;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vulkan;

namespace Vit.Framework.Windowing.Sdl;

class VulkanWindow : SdlWindow, IVulkanWindow {
	public VulkanWindow ( SdlHost host ) : base( host, GraphicsApiType.Vulkan ) { }

	public override Size2<uint> PixelSize {
		get {
			SDL.SDL_Vulkan_GetDrawableSize( Pointer, out int pixelWidth, out int pixelHeight );
			return new( (uint)pixelWidth, (uint)pixelHeight );
		}
	}

	protected override void InitializeHints ( ref SDL.SDL_WindowFlags flags ) {
		flags |= SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN;
	}

	bool swapchainCreated;
	public override (ISwapchain swapchain, IRenderer renderer) CreateSwapchain ( GraphicsApi api, SwapChainArgs args ) {
		if ( swapchainCreated )
			throw new NotImplementedException( "Surface recreation not implemented" );
		swapchainCreated = true;

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
