using SDL2;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Graphics.Vulkan.Windowing;
using Vit.Framework.Mathematics;
using Vulkan;

namespace Vit.Framework.Windowing.Sdl;

class VulkanWindow : SdlWindow, IVulkanWindow {
	public VulkanWindow ( SdlHost host ) : base( host, GraphicsApiType.Vulkan ) { }

	public override Size2<int> PixelSize {
		get {
			SDL.SDL_Vulkan_GetDrawableSize( Pointer, out int pixelWidth, out int pixelHeight );
			return new( pixelWidth, pixelHeight );
		}
	}

	public VkSurfaceKHR GetSurface ( VulkanInstance vulkan ) {
		SDL.SDL_Vulkan_CreateSurface( Pointer, vulkan.Handle.Handle, out var surface );
		return new VkSurfaceKHR( (ulong)surface );
	}
}
