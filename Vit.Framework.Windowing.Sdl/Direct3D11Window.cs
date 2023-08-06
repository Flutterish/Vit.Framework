using SDL2;
using Vit.Framework.Graphics.Direct3D11;
using Vit.Framework.Graphics.Direct3D11.Windowing;
using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Windowing.Sdl;

class Direct3D11Window : SdlWindow, IDirect3D11Window {
	public Direct3D11Window ( SdlHost host ) : base( host, Direct3D11Api.GraphicsApiType ) { }

	protected override void InitializeHints ( ref SDL.SDL_WindowFlags flags ) {
		
	}

	bool swapchainCreated;
	public override Task<WindowGraphicsSurface> CreateGraphicsSurface ( GraphicsApi api, WindowSurfaceArgs args ) {
		if ( swapchainCreated )
			throw new NotImplementedException( "Surface recreation not implemented" );
		swapchainCreated = true;
		
		if ( api is not Direct3D11Api dx )
			throw new ArgumentException( "Graphics API must be an Direct3D11 API created from the same host as this window", nameof( api ) );

		return Task.FromResult<WindowGraphicsSurface>( new Direct3D11WindowSurface( dx, args, this ) );
	}

	public nint GetWindowPointer () {
		SDL.SDL_SysWMinfo info = default;
		SDL.SDL_VERSION( out info.version );
		SDL.SDL_GetWindowWMInfo( Pointer, ref info );

		return info.info.win.window;
	}
}