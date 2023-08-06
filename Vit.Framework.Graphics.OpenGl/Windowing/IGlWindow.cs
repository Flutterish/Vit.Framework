using Vit.Framework.Windowing;

namespace Vit.Framework.Graphics.OpenGl.Windowing;

public interface IGlWindow : IWindow {
	nint CreateContext ();
	void MakeCurrent ( nint context );

	void SwapBackbuffer ();
}
