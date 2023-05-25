using Vit.Framework.Input;

namespace Vit.Framework.Windowing.Sdl.Input;

public class CursorTracker : CursorState.Tracker {
	public CursorTracker ( SdlWindow window ) {
		window.CursorMoved += position => ScheduleUpdate( new() {
			Timestamp = DateTime.Now,
			Type = CursorState.DeltaType.Position,
			Position = position
		} );

		window.MouseButtonStateChanged += ( button, state ) => {
			if ( button >= CursorState.ButtonCount )
				return;

			CursorState.Delta delta = new() {
				Timestamp = DateTime.Now,
				Type = CursorState.DeltaType.Buttons,
			};
			delta.ButtonsChanged[button] = true;
			delta.ButtonsDown[button] = state;
			ScheduleUpdate( delta );
		};
	}
}
