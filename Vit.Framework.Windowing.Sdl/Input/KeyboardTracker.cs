using SDL2;
using Vit.Framework.Input;

namespace Vit.Framework.Windowing.Sdl.Input;

public class KeyboardTracker : KeyboardState.Tracker {
	SdlWindow window;
	public KeyboardTracker ( SdlWindow window ) {
		this.window = window;
		window.OnKeyboardEvent += onKeyboardEvent;
	}

	void onKeyboardEvent ( SDL.SDL_KeyboardEvent e ) {
		KeyboardState.Delta delta = new() { Timestamp = DateTime.Now }; // TODO use SDL timestamps
		if ( KeyExtensions.GetKeyByScanCode( (int)e.keysym.scancode ) is Key key ) {
			delta.IsDown = e.state != 0;
			delta.Key = key;
			delta.IsRepeat = e.repeat != 0;
		}
		else return;

		ScheduleUpdate( delta );
	}

	public override void Dispose () {
		window.OnKeyboardEvent -= onKeyboardEvent;
	}
}
