using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Mathematics;
using Vit.Framework.Threading;

namespace Vit.Framework.Windowing;

public abstract class Window : IDisposable {
	public readonly RenderingApi RenderingApi;
	public Window ( RenderingApi renderingApi ) {
		RenderingApi = renderingApi;
	}

	public abstract string Title { get; set; }
	public int Width { get => Size.Width; set => Size = Size with { Width = value }; }
	public int Height { get => Size.Height; set => Size = Size with { Height = value }; }

	public abstract Size2<int> Size { get; set; }

	public int PixelWidth => PixelSize.Width;
	public int PixelHeight => PixelSize.Height;
	public virtual Size2<int> PixelSize => Size;

	protected void RegisterThread ( AppThread thread ) {
		ThreadCreated?.Invoke( thread );
	}
	public event Action<AppThread>? ThreadCreated;

	public bool IsInitialized { get; private set; }
	protected void OnInitialized () {
		IsInitialized = true;
		Initialized?.Invoke( this );
	}
	public event Action<Window>? Initialized;

	public abstract (ISwapchain swapchain, IGraphicsDevice device) CreateSwapchain ( Renderer renderer );

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