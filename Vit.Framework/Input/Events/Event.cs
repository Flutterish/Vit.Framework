namespace Vit.Framework.Input.Events;

public abstract class Event : IHasTimestamp {
	public Event ( DateTime timestamp ) {
		Timestamp = timestamp;
	}

	public DateTime Timestamp { get; }
}
