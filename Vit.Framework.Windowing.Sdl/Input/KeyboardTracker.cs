using Vit.Framework.Input;
using Vit.Framework.Input.Trackers;
using static SDL2.SDL;

namespace Vit.Framework.Windowing.Sdl.Input;

public class KeyboardTracker : KeyboardState.Tracker {
	SdlWindow window;
	public KeyboardTracker ( SdlWindow window ) {
		this.window = window;
		window.OnKeyboardEvent += onKeyboardEvent;
	}

	HashSet<SDL_Keymod> pressedModifiers = new();
	void onKeyboardEvent ( SDL_KeyboardEvent e ) {
		if ( KeyExtensions.GetKeyByScanCode( (int)e.keysym.scancode ) is Key key ) {
			ScheduleUpdate( new() {
				Timestamp = DateTime.Now, // TODO use SDL timestamps
				IsDown = e.state != 0,
				Key = key,
				IsRepeat = e.repeat != 0
			} ); 
		}

		var modifiers = e.keysym.mod;
		foreach ( var i in Enum.GetValues<SDL_Keymod>() ) {
			if ( modifiers.HasFlag( i ) && pressedModifiers.Add( i ) && translate( i ) is Key keyDown ) {
				ScheduleUpdate( new() {
					Timestamp = DateTime.Now, // TODO use SDL timestamps
					IsDown = true,
					Key = keyDown,
					IsRepeat = false
				} );
			}
			else if ( !modifiers.HasFlag( i ) && pressedModifiers.Remove( i ) && translate( i ) is Key keyUp ) {
				ScheduleUpdate( new() {
					Timestamp = DateTime.Now, // TODO use SDL timestamps
					IsDown = false,
					Key = keyUp,
					IsRepeat = false
				} );
			}
		}
	}

	Key? translate ( SDL_Keymod single ) {
		return single switch {
			SDL_Keymod.KMOD_LSHIFT => Key.LeftShift,
			SDL_Keymod.KMOD_RSHIFT => Key.RightShift,
			SDL_Keymod.KMOD_LCTRL => Key.LeftControl,
			SDL_Keymod.KMOD_RCTRL => Key.RightControl,
			SDL_Keymod.KMOD_LALT => Key.Alt,
			//SDL_Keymod.KMOD_RALT => Key.AltGr,
			SDL_Keymod.KMOD_LGUI => Key.LeftHost,
			SDL_Keymod.KMOD_RGUI => Key.RightHost,
			SDL_Keymod.KMOD_NUM => Key.NumLock,
			SDL_Keymod.KMOD_CAPS => Key.CapsLock,
			SDL_Keymod.KMOD_MODE => Key.AltGr,
			_ => null,
		};
	}

	public override void Dispose () {
		window.OnKeyboardEvent -= onKeyboardEvent;
	}
}
