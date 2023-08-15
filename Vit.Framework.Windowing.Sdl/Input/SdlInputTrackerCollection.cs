using Vit.Framework.Input;

namespace Vit.Framework.Windowing.Sdl.Input;

public class SdlInputTrackerCollection : InputTrackerCollection {
	public SdlInputTrackerCollection ( SdlWindow window ) {
		OnTrackerDetected( new CursorTracker( window ) );
		OnTrackerDetected( new TextInputTracker( window ) );
		OnTrackerDetected( new KeyboardTracker( window ) );
	}

	public override void Dispose () {
		foreach ( var i in Trackers ) {
			if ( i is IDisposable d )
				d.Dispose();
		}
	}
}
