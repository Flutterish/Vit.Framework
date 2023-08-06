using SDL2;
using Vit.Framework.Graphics.Direct3D11;
using Vit.Framework.Graphics.Direct3D11.Windowing;
using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Windowing.Sdl.Backends;

class DirectXBackend : SdlBackend {
	public override void InitializeHints ( WindowSurfaceArgs args, ref SDL.SDL_WindowFlags flags ) { }

	public override WindowGraphicsSurface CreateSurface ( GraphicsApi api, WindowSurfaceArgs args, SdlWindow window ) {
		if ( api is not Direct3D11Api dx )
			throw new ArgumentException( "Graphics API must be an Direct3D11 API created from the same host as this window", nameof( api ) );

		return new Direct3D11WindowSurface( dx, args, window );
	}
}
