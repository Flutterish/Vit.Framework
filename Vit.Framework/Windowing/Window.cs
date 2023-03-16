using Vit.Framework.Math;

namespace Vit.Framework.Windowing;

public abstract class Window : IDisposable {
	public abstract int Width { get; set; }
	public abstract int Height { get; set; }

	public Size2<int> Size {
		get => new( Width, Height );
		set {
			Width = value.Width;
			Height = value.Height;
		}
	}

	public bool IsClosed { get; private set; }
	protected abstract void Dispose ( bool disposing );

	~Window () {
		Dispose( disposing: false );
		IsClosed = true;
	}

	public void Dispose () {
		Dispose( disposing: true );
		IsClosed = true;
		GC.SuppressFinalize( this );
	}
}
