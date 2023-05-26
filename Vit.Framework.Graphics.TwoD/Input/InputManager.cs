using Vit.Framework.Input;

namespace Vit.Framework.Graphics.TwoD.Input;

public class InputManager {
	public required IDrawable Root { get; init; }
	public IInputTracker<CursorState>? CursorTracker { get; init; }

	public void Update () {
		if ( CursorTracker != null ) {
			CursorTracker.Update();
		}
	}
}
