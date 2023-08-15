using SDL2;
using Vit.Framework.Input;
using Vit.Framework.Input.Trackers;

namespace Vit.Framework.Windowing.Sdl.Input;

public class CursorTracker : CursorState.Tracker, IDisposable {
	SdlWindow window;
	public CursorTracker ( SdlWindow window ) {
		this.window = window;
		window.CursorMoved += onCursorMoved;
		window.MouseButtonStateChanged += onMouseButtonStateChanged;
	}

	void onMouseButtonStateChanged ( byte button, bool state ) {
		var index = (uint)button switch {
			SDL.SDL_BUTTON_LEFT => (int)CursorButton.Left,
			SDL.SDL_BUTTON_RIGHT => (int)CursorButton.Right,
			SDL.SDL_BUTTON_MIDDLE => (int)CursorButton.Middle,
			SDL.SDL_BUTTON_X1 => (int)CursorButton.Extra1,
			SDL.SDL_BUTTON_X2 => (int)CursorButton.Extra2,
			_ => -1
		};

		if ( index == -1 )
			return;

		CursorState.Delta delta = new() {
			Timestamp = DateTime.Now, // TODO use SDL timestamps
			Type = CursorState.DeltaType.Buttons,
		};
		delta.ButtonsChanged[index] = true;
		delta.ButtonsDown[index] = state;
		ScheduleUpdate( delta );
	}

	private void onCursorMoved ( Mathematics.Point2<float> position ) {
		ScheduleUpdate( new() {
			Timestamp = DateTime.Now, // TODO use SDL timestamps
			Type = CursorState.DeltaType.Position,
			Position = (position.X, window.Height - position.Y)
		} );
	}

	public override void Dispose () {
		window.CursorMoved -= onCursorMoved;
		window.MouseButtonStateChanged -= onMouseButtonStateChanged;
	}
}
