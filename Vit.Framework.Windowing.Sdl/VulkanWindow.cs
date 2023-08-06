using SDL2;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Graphics.Vulkan.Windowing;
using Vit.Framework.Mathematics;
using Vulkan;

namespace Vit.Framework.Windowing.Sdl;

class VulkanWindow : SdlWindow, IVulkanWindow {
	public VulkanWindow ( SdlHost host ) : base( host, VulkanApi.GraphicsApiType ) { }

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
	public override Task<WindowGraphicsSurface> CreateGraphicsSurface ( GraphicsApi api, WindowSurfaceArgs args ) {
		if ( swapchainCreated )
			throw new NotImplementedException( "Surface recreation not implemented" );
		swapchainCreated = true;

		if ( api is not VulkanApi vulkan )
			throw new ArgumentException( "Graphics API must be a Vulkan API created from the same host as this window", nameof(api) );

		return Task.FromResult<WindowGraphicsSurface>( new VulkanWindowSurface( vulkan, args, this ) );
	}

	public VkSurfaceKHR GetSurface ( VulkanInstance vulkan ) {
		SDL.SDL_Vulkan_CreateSurface( Pointer, vulkan.Handle.Handle, out var surface );
		return new VkSurfaceKHR( (ulong)surface );
	}
}
