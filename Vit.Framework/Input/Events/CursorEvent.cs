using Vit.Framework.Input;
using Vit.Framework.Input.Events;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Input.Events;

public abstract record CursorEvent : Event {
	public required CursorState CursorState { get; init; }
}

public record CursorMovedEvent : CursorEvent {
	public Point2<float> LastPosition => CursorState.LastScreenSpacePosition;
	public Point2<float> Position => CursorState.ScreenSpacePosition;
	public Vector2<float> DeltaPosition => CursorState.DeltaScreenSpacePosition;
}

public record CursorButtonPressedEvent : CursorEvent {
	public required CursorButton Button { get; init; }
}

public record CursorButtonReleasedEvent : CursorEvent {
	public required CursorButton Button { get; init; }
}