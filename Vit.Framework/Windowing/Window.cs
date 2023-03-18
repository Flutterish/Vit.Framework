using Vit.Framework.Mathematics;
using Vit.Framework.Threading;

namespace Vit.Framework.Windowing;

public abstract class Window : IDisposable {
	public abstract string Title { get; set; }
	public int Width { get => Size.Width; set => Size = Size with { Width = value }; }
	public int Height { get => Size.Height; set => Size = Size with { Height = value }; }

	public abstract Size2<int> Size { get; set; }

	protected void RegisterThread ( AppThread thread ) {
		ThreadCreated?.Invoke( thread );
	}
	public event Action<AppThread>? ThreadCreated;

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
