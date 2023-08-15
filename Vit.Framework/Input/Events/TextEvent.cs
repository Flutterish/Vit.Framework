namespace Vit.Framework.Input.Events;

public abstract record TextEvent : TimestampedEvent {
	public required TextInput State { get; init; }
}

public record TextInputEvent : TextEvent {
	public required string Text { get; init; }
}