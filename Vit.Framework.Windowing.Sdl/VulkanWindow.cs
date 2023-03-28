using SDL2;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Graphics.Vulkan.Queues;
using Vit.Framework.Mathematics;
using Vulkan;

namespace Vit.Framework.Windowing.Sdl;

class VulkanWindow : SdlWindow {
	public VulkanWindow ( SdlHost host ) : base( host, RenderingApi.Vulkan ) { }

	public override Size2<int> PixelSize {
		get {
			SDL.SDL_Vulkan_GetDrawableSize( Pointer, out int pixelWidth, out int pixelHeight );
			return new( pixelWidth, pixelHeight );
		}
	}

	public override (ISwapchain swapchain, IGraphicsDevice device) CreateSwapchain ( Renderer renderer ) {
		var vulkan = ((VulkanRenderer)renderer)!.Instance;
		var surface = createSurface( vulkan );
		return Swapchain.Create( vulkan, surface, this );
	}

	VkSurfaceKHR createSurface ( VulkanInstance vulkan ) {
		SDL.SDL_Vulkan_CreateSurface( Pointer, vulkan.Instance.Handle, out var surfacePtr );
		return new( (ulong)surfacePtr );
	}
}
