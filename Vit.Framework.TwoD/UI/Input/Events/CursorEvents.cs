using Vit.Framework.Input;
using Vit.Framework.Input.Events;
using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.UI.Input.Events;

public abstract record PositionalUIEvent : UIEvent, IPositionalEvent {
	public required Point2<float> EventPosition { get; init; }
}

public abstract record MovingPositionalUIEvent : PositionalUIEvent {
	public required Point2<float> EventStartPosition { get; init; }
	public required Point2<float> LastEventPosition { get; init; }
	public Vector2<float> DeltaPosition => EventPosition - LastEventPosition;
}

/// <summary>
/// A cursor is now over this element. Needs to handle <see cref="HoveredEvent"/> to trigger.
/// </summary>
public record CursorEnteredEvent : PositionalUIEvent, INonPropagableEvent {

}

/// <summary>
/// A cursor is no longer over this element. Needs to handle <see cref="HoveredEvent"/> to trigger.
/// </summary>
public record CursorExitedEvent : PositionalUIEvent, INonPropagableEvent {

}

/// <summary>
/// A cursor moved while over this element.
/// </summary>
public record HoveredEvent : PositionalUIEvent {

}

/// <summary>
/// A cursor started holding down a button over this element. Needs to handle <see cref="HoveredEvent"/> to trigger.
/// </summary>
public record PressedEvent : PositionalUIEvent, ILoggableEvent, INonPropagableEvent {
	public required CursorButton Button { get; init; }
}

/// <summary>
/// A cursor stopped holding down a button that was previously handled by <see cref="PressedEvent"/>.
/// </summary>
public record ReleasedEvent : PositionalUIEvent, ILoggableEvent, INonPropagableEvent {
	public required CursorButton Button { get; init; }
}

/// <summary>
/// A cursor pressed and released a button over this element. Must have handled <see cref="PressedEvent"/> for this to trigger. 
/// Handling <see cref="DragStartedEvent"/> will cause this event not to trigger.
/// </summary>
public record ClickedEvent : PositionalUIEvent, ILoggableEvent, INonPropagableEvent {
	public required CursorButton Button { get; init; }
}

/// <summary>
/// A cursor pressed a button over this element and moved. Needs to either handle <see cref="PressedEvent"/> or fall through to trigger. 
/// Handling this event will cause <see cref="ClickedEvent"/> not to trigger.
/// </summary>
public record DragStartedEvent : PositionalUIEvent, ILoggableEvent, IUpPropagableEvent {
	public required CursorButton Button { get; init; }
}

/// <summary>
/// A cursor moved while dragging this element. Must have handled <see cref="DragStartedEvent"/> for this to trigger. 
/// </summary>
public record DraggedEvent : MovingPositionalUIEvent, INonPropagableEvent {
	public required CursorButton Button { get; init; }
}

/// <summary>
/// A cursor released a button while dragging this element. Must have handled <see cref="DragStartedEvent"/> for this to trigger. 
/// </summary>
public record DragEndedEvent : MovingPositionalUIEvent, ILoggableEvent, INonPropagableEvent {
	public required CursorButton Button { get; init; }
}
