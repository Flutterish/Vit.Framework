namespace Vit.Framework.Input.Events;

public abstract record Event { }

public abstract record TimestampedEvent : Event, IHasTimestamp {
	public required DateTime Timestamp { get; init; }
}
