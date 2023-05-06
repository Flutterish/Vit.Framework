using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Input;
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

	public bool IsInitialized { get; private set; }
	protected void OnInitialized () {
		IsInitialized = true;
		initialized?.Invoke( this );
		initialized = null;
	}
	public event Action<Window>? initialized;
	public event Action<Window>? Initialized {
		add {
			if ( IsInitialized )
				value?.Invoke( this );
			else
				initialized += value;
		}
		remove {
			initialized -= value;
		}
	}

	public Point2<double> CursorPosition { get; private set; }
	protected void OnCursorMoved ( Point2<double> position ) {
		CursorPosition = position;
		CursorMoved?.Invoke( position );
	}
	public event Action<Point2<double>>? CursorMoved;

	protected void OnPhysicalKeyDown ( Key key ) {
		PhysicalKeyDown?.Invoke( key );
	}
	public event Action<Key>? PhysicalKeyDown;

	protected void OnPhysicalKeyUp ( Key key ) {
		PhysicalKeyUp?.Invoke( key );
	}
	public event Action<Key>? PhysicalKeyUp;

	// TODO try to split this to just create the swapchain?
	public abstract (ISwapchain swapchain, IRenderer renderer) CreateSwapchain ( GraphicsApi api, SwapChainArgs args );

	public bool IsClosed { get; private set; }
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
		GC.SuppressFinalize( this );
	}
}