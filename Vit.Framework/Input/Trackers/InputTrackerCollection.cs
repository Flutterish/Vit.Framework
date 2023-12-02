namespace Vit.Framework.Input.Trackers;

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
		lock ( pending ) {
			anyChanges = true;
			pending.Enqueue( (tracker, false) );
		}
	}

	public void Update () {
		if ( !anyChanges )
			return;

		lock ( pending ) {
			while ( pending.TryDequeue( out var update ) ) {
				var (tracker, isAlive) = update;
				if ( isAlive ) {
					inputTrackers.Add( tracker );
					TrackerDetected?.Invoke( tracker );
				}
				else {
					inputTrackers.Remove( tracker );
					TrackerLost?.Invoke( tracker );
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

	public void BindTrackerState ( Action<IInputTracker> detected, Action<IInputTracker> lost ) {
		TrackerDetected += detected;
		TrackerLost += lost;
		foreach ( var i in inputTrackers ) {
			detected( i );
		}
	}

	public event Action<IInputTracker>? TrackerDetected;
	public event Action<IInputTracker>? TrackerLost;

	public abstract void Dispose ();
}
