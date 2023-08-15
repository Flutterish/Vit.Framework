using Vit.Framework.Input.Events;

namespace Vit.Framework.Input;

/// <summary>
/// Queues inputs from a separate input thread and tracks them on another.
/// </summary>
/// <typeparam name="TUpdate">A change in the input state.</typeparam>
/// <typeparam name="TInput">The input state.</typeparam>
public abstract class InputTracker<TUpdate, TInput> : IInputTracker<TInput> 
	where TUpdate : IHasTimestamp where TInput : IHasTimestamp 
{
	bool enabled = true;
	public bool IsEnabled {
		get => enabled;
		set {
			enabled = value;
			if ( !value ) {
				lock ( scheduledUpdates ) {
					scheduledUpdates.Clear();
				}
			}
		}
	}

	Queue<TUpdate> scheduledUpdates = new();
	Queue<TUpdate> updates = new();

	/// <summary>
	/// Schedules an update to be realised on the next poll.
	/// </summary>
	protected void ScheduleUpdate ( TUpdate update ) {
		if ( !IsEnabled )
			return;

		lock ( scheduledUpdates ) {
			scheduledUpdates.Enqueue( update );
		}
	}

	public abstract TInput State { get; }
	public void Update () {
		lock ( scheduledUpdates ) {
			(updates, scheduledUpdates) = (scheduledUpdates, updates);
		}

		while ( updates.TryDequeue( out var update ) ) {
			Update( update );
			InputChanged?.Invoke( this, State );
			foreach ( var e in EmitEvents( update ) ) {
				InputEventEmitted?.Invoke( this, e );
			}
		}
	}

	/// <summary>
	/// Updates the input state <strong>and its timestamp</strong>.
	/// </summary>
	protected abstract void Update ( TUpdate update );

	/// <summary>
	/// Emits events related to the change.
	/// </summary>
	protected abstract IEnumerable<TimestampedEvent> EmitEvents ( TUpdate update );

	public event Action<IInputTracker, TInput>? InputChanged;
	public event Action<IInputTracker, TimestampedEvent>? InputEventEmitted;

	public abstract void Dispose ();
}

public interface IInputTracker<TInput> : IInputTracker where TInput : IHasTimestamp {
	event Action<IInputTracker, TInput>? InputChanged;
}

public interface IInputTracker {
	/// <summary>
	/// Polls all pending changes and triggers related events.
	/// </summary>
	void Update ();

	/// <summary>
	/// Controls if this tracker will process changes. <see langword="true"/> by default. 
	/// You need to disable it manually to avoid allocating memeory for changes that will never be collected.
	/// </summary>
	bool IsEnabled { get; set; }

	event Action<IInputTracker, TimestampedEvent>? InputEventEmitted;
}