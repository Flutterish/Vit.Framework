using SDL2;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Graphics.Vulkan.Windowing;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Windowing.Sdl.Backends;

class VulkanBackend : SdlBackend {
	public override void InitializeHints ( WindowSurfaceArgs args, ref SDL.SDL_WindowFlags flags ) {
		flags |= SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN;
	}

	public override WindowGraphicsSurface CreateSurface ( GraphicsApi api, WindowSurfaceArgs args, SdlWindow window ) {
		if ( api is not VulkanApi vulkan )
			throw new ArgumentException( "Graphics API must be a Vulkan API created from the same host as this window", nameof( api ) );

		return new VulkanWindowSurface( vulkan, args, window );
	}

	public override Size2<uint> GetPixelSize ( SdlWindow window ) {
		SDL.SDL_Vulkan_GetDrawableSize( window.Pointer, out int pixelWidth, out int pixelHeight );
		return new( (uint)pixelWidth, (uint)pixelHeight );
	}
}
