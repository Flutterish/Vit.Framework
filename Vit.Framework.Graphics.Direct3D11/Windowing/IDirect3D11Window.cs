using Vit.Framework.Windowing;

namespace Vit.Framework.Graphics.Direct3D11.Windowing;

public interface IDirect3D11Window : IWindow {
	nint GetWindowPointer ();
}
