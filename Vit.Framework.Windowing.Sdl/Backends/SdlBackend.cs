using SDL2;
using Vit.Framework.Graphics.Direct3D11;
using Vit.Framework.Graphics.OpenGl;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Windowing.Sdl.Backends;

abstract class SdlBackend {
	static VulkanBackend? vulkanBackend;
	static DirectXBackend? directXBackend;
	static GlBackend? glBackend;
	public static readonly NullSdlBackend Null = new();

	public static SdlBackend GetBackend ( GraphicsApiType api ) {
		return api switch {
			var x when x == VulkanApi.GraphicsApiType => vulkanBackend ??= new(),
			var x when x == Direct3D11Api.GraphicsApiType => directXBackend ??= new(),
			var x when x == OpenGlApi.GraphicsApiType => glBackend ??= new(),
			_ => throw new ArgumentException( $"Unsupported rendering api: {api}", nameof( api ) )
		};
	}

	public abstract void InitializeHints ( WindowSurfaceArgs args, ref SDL.SDL_WindowFlags flags );
	public abstract WindowGraphicsSurface CreateSurface ( GraphicsApi api, WindowSurfaceArgs args, SdlWindow window );
	public virtual Size2<uint> GetPixelSize ( SdlWindow window ) => window.Size;
}

class NullSdlBackend : SdlBackend {
	public override void InitializeHints ( WindowSurfaceArgs args, ref SDL.SDL_WindowFlags flags ) {
		flags |= SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN; // HACK so you can create the vulkan api
	}

	public override WindowGraphicsSurface CreateSurface ( GraphicsApi api, WindowSurfaceArgs args, SdlWindow window ) {
		throw new NotImplementedException();
	}
}