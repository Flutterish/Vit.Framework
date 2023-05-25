using SDL2;
using Vit.Framework.Input;

namespace Vit.Framework.Windowing.Sdl.Input;

public class CursorTracker : CursorState.Tracker {
	public CursorTracker ( SdlWindow window ) { // TODO use SDL timestamps?
		window.CursorMoved += position => ScheduleUpdate( new() {
			Timestamp = DateTime.Now,
			Type = CursorState.DeltaType.Position,
			Position = position
		} );

		window.MouseButtonStateChanged += ( button, state ) => {
			var index = (uint)button switch {
				SDL.SDL_BUTTON_LEFT => (int)MouseButton.Left,
				SDL.SDL_BUTTON_RIGHT => (int)MouseButton.Right,
				SDL.SDL_BUTTON_MIDDLE => (int)MouseButton.Middle,
				SDL.SDL_BUTTON_X1 => (int)MouseButton.Extra1,
				SDL.SDL_BUTTON_X2 => (int)MouseButton.Extra2,
				_ => -1
			};

			if ( index == -1 )
				return;

			CursorState.Delta delta = new() {
				Timestamp = DateTime.Now,
				Type = CursorState.DeltaType.Buttons,
			};
			delta.ButtonsChanged[index] = true;
			delta.ButtonsDown[index] = state;
			ScheduleUpdate( delta );
		};
	}
}
