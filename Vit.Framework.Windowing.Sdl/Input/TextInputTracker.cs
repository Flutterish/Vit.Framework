using Vit.Framework.Input;

namespace Vit.Framework.Windowing.Sdl.Input;

public class TextInputTracker : TextInput.Tracker {
	SdlWindow window;
	public TextInputTracker ( SdlWindow window ) {
		this.window = window;
		window.OnTextInput += onTextInput;
	}

	void onTextInput ( string str ) {
		ScheduleUpdate( new() {
			Timestamp = DateTime.Now, // TODO use SDL timestamps
			Value = str
		} );
	}

	public override void Dispose () {
		window.OnTextInput -= onTextInput;
	}
}
