using Vit.Framework.Windowing;

namespace Vit.Framework.Graphics.OpenGl;

public interface IGlWindow : IWindow {
	nint CreateContext ();
	void MakeCurrent ( nint context );

	void SwapBackbuffer ();
}
