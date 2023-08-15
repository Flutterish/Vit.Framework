using System.Collections;

namespace Vit.Framework.Input;

/// <summary>
/// A collection of input trackers, which can be dynamically detected or lost.
/// </summary>
public abstract class InputTrackerCollection : IDisposable {
	HashSet<IInputTracker> inputTrackers = new();
	Queue<(IInputTracker tracker, bool isAlive)> pending = new();
	bool anyChanges;
	protected void OnTrackerDetected ( IInputTracker tracker ) {
		lock ( pending ) {
			anyChanges = true;
			pending.Enqueue( (tracker, true) );
		}
	}

	protected void OnTrackerLost ( IInputTracker tracker ) {
		lock( pending ) {
			anyChanges = true;
			pending.Enqueue( (tracker, false) );
		}
	}

	public void Update ( Action<IInputTracker>? detected = null, Action<IInputTracker>? lost = null ) {
		if ( !anyChanges )
			return;

		lock ( pending ) {
			while ( pending.TryDequeue( out var update ) ) {
				var (tracker, isAlive) = update;
				if ( isAlive ) {
					inputTrackers.Add( tracker );
					detected?.Invoke( tracker );
				}
				else {
					inputTrackers.Remove( tracker );
					lost?.Invoke( tracker );
				}
			}

			anyChanges = false;
		}
	}

	public IEnumerable<IInputTracker> Trackers {
		get {
			Update();
			return inputTrackers;
		}
	}

	public abstract void Dispose ();
}
