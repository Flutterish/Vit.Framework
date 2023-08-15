namespace Vit.Framework.Input.Events;

public abstract record KeyEvent : TimestampedEvent {
	public required KeyboardState State { get; init; }
	public required Key Key { get; init; }
}

public record KeyDownEvent : KeyEvent { }
public record KeyRepeatEvent : KeyEvent { }
public record KeyUpEvent : KeyEvent { }
