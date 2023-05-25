namespace Vit.Framework.Input;

/// <summary>
/// Queues inputs from a separate input thread and tracks them on another.
/// </summary>
/// <typeparam name="TUpdate">A change in the input state.</typeparam>
/// <typeparam name="TInput">The input state.</typeparam>
public abstract class InputTracker<TUpdate, TInput> : IInputTracker<TInput> 
	where TUpdate : IHasTimestamp where TInput : IHasTimestamp 
{
	Queue<TUpdate> scheduledUpdates = new();
	Queue<TUpdate> updates = new();

	/// <summary>
	/// Schedules an update to be realised on the next poll.
	/// </summary>
	protected void ScheduleUpdate ( TUpdate update ) {
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
			InputChanged?.Invoke( State );
		}
	}

	/// <summary>
	/// Updates the input state <strong>and its timestamp</strong>.
	/// </summary>
	protected abstract void Update ( TUpdate update );

	public event Action<TInput>? InputChanged;
}

public interface IInputTracker<TInput> where TInput : IHasTimestamp {
	/// <summary>
	/// Polls all pending changes and triggers related events.
	/// </summary>
	void Update ();

	event Action<TInput>? InputChanged;
}