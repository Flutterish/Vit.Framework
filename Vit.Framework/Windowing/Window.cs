using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Windowing;

public abstract class Window : IWindow, IDisposable {
	public readonly GraphicsApiType RenderingApi;
	public Window ( GraphicsApiType renderingApi ) {
		RenderingApi = renderingApi;
	}

	public abstract string Title { get; set; }
	public uint Width { get => Size.Width; set => Size = Size with { Width = value }; }
	public uint Height { get => Size.Height; set => Size = Size with { Height = value }; }

	public abstract Size2<uint> Size { get; set; }

	public uint PixelWidth => PixelSize.Width;
	public uint PixelHeight => PixelSize.Height;
	public virtual Size2<uint> PixelSize => Size;

	protected void OnResized () {
		Resized?.Invoke( this );
	}
	public event Action<Window>? Resized;

	/// <summary>
	/// Creates a graphics surface representing the window based on the provided options. 
	/// If this is called for a second time, the previous surface is no longer valid.
	/// The swapchain is invalid but the renderer might - but doesn't have to - be the same object as returned last time.
	/// </summary>
	/// <remarks>
	/// Depending on the graphics backend and/or the host, this might visually close and reopen the window.
	/// </remarks>
	public abstract WindowGraphicsSurface CreateGraphicsSurface ( GraphicsApi api, WindowSurfaceArgs args );

	public bool IsClosed { get; private set; }
	public event Action<Window>? Closed;
	protected abstract void Dispose ( bool disposing );

	~Window () {
		Dispose( disposing: false );
		IsClosed = true;
	}

	public void Quit () {
		Dispose();
	}
	public void Dispose () {
		if ( IsClosed )
			return;

		Dispose( disposing: true );
		IsClosed = true;
		Closed?.Invoke( this );
		GC.SuppressFinalize( this );
	}
}