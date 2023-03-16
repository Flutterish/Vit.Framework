using SDL2;
using Vit.Framework.Windowing;

namespace Vit.Framework.SdlWindowing;

class SdlWindow : Window {
	public override int Width { get; set; }
	public override int Height { get; set; }

	nint windowPointer;
	public uint Id;
	public void Init () {
		windowPointer = SDL.SDL_CreateWindow( "New Window", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, 640, 480, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN );
		if ( windowPointer == IntPtr.Zero ) {
			SdlHost.ThrowSdl( "window creation failed" );
		}

		Id = SDL.SDL_GetWindowID( windowPointer );
	}

	protected override void Dispose ( bool disposing ) {
		SDL.SDL_DestroyWindow( windowPointer );
	}
}
