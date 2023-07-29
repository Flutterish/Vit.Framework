using Vit.Framework.Mathematics;

namespace Vit.Framework.Input.Events;

public abstract record CursorEvent : Event, IPositionalEvent {
	public required CursorState CursorState { get; init; }
	public Point2<float> EventPosition => CursorState.ScreenSpacePosition;
}

public record CursorMovedEvent : CursorEvent {
	public Point2<float> LastPosition => CursorState.LastScreenSpacePosition;
	public Point2<float> Position => CursorState.ScreenSpacePosition;
	public Vector2<float> DeltaPosition => CursorState.DeltaScreenSpacePosition;
}

public record CursorButtonPressedEvent : CursorEvent, ILoggableEvent {
	public required CursorButton Button { get; init; }
}

public record CursorButtonReleasedEvent : CursorEvent, ILoggableEvent {
	public required CursorButton Button { get; init; }
}