using Vit.Framework.Input;
using Vit.Framework.Input.Events;
using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.Input.Events;

public abstract record PositionalUIEvent : UIEvent, IPositionalEvent {
	public required Point2<float> EventPosition { get; init; }
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
/// </summary>
public record ClickedEvent : PositionalUIEvent, ILoggableEvent, INonPropagableEvent {
	public required CursorButton Button { get; init; }
}