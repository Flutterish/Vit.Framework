using SDL2;
using Vit.Framework.Graphics.Direct3D11.Windowing;
using Vit.Framework.Graphics.OpenGl.Windowing;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Graphics.Vulkan.Windowing;
using Vit.Framework.Input.Trackers;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vit.Framework.Windowing.Sdl.Backends;
using Vit.Framework.Windowing.Sdl.Input;
using Vulkan;

namespace Vit.Framework.Windowing.Sdl;

public class SdlWindow : Window, IGlWindow, IDirect3D11Window, IVulkanWindow {
	string title = "New Window";
	public override string Title {
		get => title;
		set {
			title = value;
			if ( Pointer == 0 )
				return;

			host.Shedule( () => SDL.SDL_SetWindowTitle( Pointer, title ) );
		}
	}

	Size2<uint> size = new( 640, 480 );
	public override Size2<uint> Size {
		get => size;
		set {
			size = value;
			if ( Pointer == 0 )
				return;

			host.Shedule( () => {
				SDL.SDL_SetWindowSize( Pointer, (int)Width, (int)Height );
				OnResized();
			} );
		}
	}

	public override Size2<uint> PixelSize => backend.GetPixelSize( this );

	SdlHost host;
	public nint Pointer;
	public uint Id;
	public SdlWindow ( SdlHost host ) {
		this.host = host;
	}

	public void Init () {
		create();
	}

	internal SdlBackend backend = SdlBackend.Null;
	WindowSurfaceArgs surfaceArgs;
	GraphicsApi? api;
	void create () {
		var windowFlags = SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
		backend.InitializeHints( surfaceArgs, ref windowFlags );

		Pointer = SDL.SDL_CreateWindow( title, SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, (int)Width, (int)Height, windowFlags );
		if ( Pointer == 0 ) {
			SdlHost.ThrowSdl( "window creation failed" );
		}

		Id = SDL.SDL_GetWindowID( Pointer );
		SdlWindowCreated?.Invoke( this );
	}

	public override async Task<WindowGraphicsSurface> CreateGraphicsSurface ( GraphicsApi api, WindowSurfaceArgs args ) {
		backend = SdlBackend.GetBackend( api.Type );
		this.surfaceArgs = args;
		this.api = api;
		await Recreate();

		return backend.CreateSurface( api, args, this );
	}

	public override InputTrackerCollection CreateInputTrackers () {
		return new SdlInputTrackerCollection( this );
	}

	protected Task Recreate () {
		var source = new TaskCompletionSource();
		host.Shedule( () => {
			host.destroyWindow( this );
			create();
			source.SetResult();
		} );
		return source.Task;
	}

	internal event Action<SdlWindow>? SdlWindowCreated;

	public void OnEvent ( SDL.SDL_WindowEvent e ) {
		if ( e.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE )
			Quit();
		else if ( e.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED ) {
			SDL.SDL_GetWindowSize( Pointer, out var width, out var height );
			size.Width = (uint)width;
			size.Height = (uint)height;
			OnResized();
		}
	}

	public void OnEvent ( SDL.SDL_MouseMotionEvent e ) {
		CursorMoved?.Invoke( new( e.x, e.y ) );
	}
	public event Action<Point2<float>>? CursorMoved;

	public void OnEvent ( SDL.SDL_MouseButtonEvent e ) {
		MouseButtonStateChanged?.Invoke( e.button, e.state != 0 );
	}
	public event Action<byte, bool>? MouseButtonStateChanged;

	public void OnEvent ( SDL.SDL_MouseWheelEvent e ) {

	}

	public void OnEvent ( SDL.SDL_KeyboardEvent e ) {
		OnKeyboardEvent?.Invoke( e );
	}
	public event Action<SDL.SDL_KeyboardEvent>? OnKeyboardEvent;

	public unsafe void OnEvent ( SDL.SDL_TextInputEvent e ) {
		OnTextInput?.Invoke( new CString( e.text ).ToString() );
	}
	public event Action<string>? OnTextInput;

	public void CheckResize () {
		SDL.SDL_GetWindowSize( Pointer, out var width, out var height );
		var newSize = new Size2<uint>( (uint)width, (uint)height );
		if ( size != newSize ) {
			size = newSize;
			OnResized();
		}
	}

	protected override void Dispose ( bool disposing ) {
		host.Shedule( () => {
			host.destroyWindow( this );
		} );
	}

	#region opengl
	public nint CreateContext () {
		var context = SDL.SDL_GL_CreateContext( Pointer );
		if ( context == 0 )
			SdlHost.ThrowSdl( "gl context creation" );

		return context;
	}
	public void MakeCurrent ( nint context ) {
		SDL.SDL_GL_MakeCurrent( Pointer, context );
	}
	public void SwapBackbuffer () {
		SDL.SDL_GL_SwapWindow( Pointer );
	}
	#endregion
	#region direct3d
	public nint GetWindowPointer () {
		SDL.SDL_SysWMinfo info = default;
		SDL.SDL_VERSION( out info.version );
		SDL.SDL_GetWindowWMInfo( Pointer, ref info );

		return info.info.win.window;
	}
	#endregion
	#region vulkan
	public VkSurfaceKHR GetSurface ( VulkanInstance vulkan ) {
		SDL.SDL_Vulkan_CreateSurface( Pointer, vulkan.Handle.Handle, out var surface );
		return new VkSurfaceKHR( (ulong)surface );
	}
	#endregion
}