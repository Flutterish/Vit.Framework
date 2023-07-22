using Vit.Framework.Input;
using Vit.Framework.Input.Events;

namespace Vit.Framework.Graphics.TwoD.Input;

public class GlobalInputTrackers {
	public required IDrawable Root { get; init; }

	List<IInputTracker> trackers = new();
	public void Add ( IInputTracker tracker ) {
		trackers.Add( tracker );
		tracker.InputEventEmitted += OnInputEventEmitted;
	}

	private void OnInputEventEmitted ( Event e ) {
		EventEmitted?.Invoke( e );
	}

	public void Update () {
		foreach ( var i in trackers ) {
			i.Update();
		}
	}

	public event Action<Event>? EventEmitted;
}
