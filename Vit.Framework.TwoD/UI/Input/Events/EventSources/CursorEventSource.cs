using Vit.Framework.Input;
using Vit.Framework.Input.Events;
using Vit.Framework.Input.Trackers;
using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.UI.Input.Events.EventSources;

public class CursorEventSource<THandler> where THandler : class, IHasEventTrees<THandler>, ICanReceivePositionalInput {
	public required THandler Root { get; init; }
	Dictionary<CursorButton, (THandler handler, Point2<float> startPosition)> pressHandlers = new();
	public float DragDeadzone = 10;

	public THandler? Hovered { get; private set; }

	public THandler? Dragged { get; private set; }
	CursorButton dragButton;
	Point2<float> dragStartPosition;

	public void Press ( CursorState state, CursorButton button, Millis timestamp ) {
		Release( state, button, timestamp );

		if ( Hovered == null || !Hovered.TriggerEventOnSelf( new PressedEvent { Button = button, EventPosition = state.ScreenSpacePosition, Timestamp = timestamp } ) )
			return;

		pressHandlers[button] = (Hovered, state.ScreenSpacePosition);
	}

	public void Release ( CursorState state, CursorButton button, Millis timestamp, Action<THandler>? clicked = null ) {
		bool releasedDrag = Dragged != null && button == dragButton;
		if ( releasedDrag ) {
			Dragged!.TriggerEventOnSelf( new DragEndedEvent { Button = dragButton, EventPosition = state.ScreenSpacePosition, EventStartPosition = dragStartPosition, LastEventPosition = state.LastScreenSpacePosition, Timestamp = timestamp } );
			Dragged = null;
		}

		if ( !pressHandlers.Remove( button, out var previousHandler ) )
			return;

		bool result = previousHandler.handler.TriggerEventOnSelf( new ReleasedEvent { Button = button, EventPosition = state.ScreenSpacePosition, Timestamp = timestamp } );
		if ( !releasedDrag && previousHandler.handler == Hovered && previousHandler.handler.TriggerEventOnSelf( new ClickedEvent { Button = button, EventPosition = state.ScreenSpacePosition, Timestamp = timestamp } ) )
			clicked?.Invoke( previousHandler.handler );
	}

	public void Move ( CursorState state, Millis timestamp ) { // TODO could we reuse/pool events? making one essentially every frame sounds bad
		var handler = Root.TriggerCulledEvent( new HoveredEvent { EventPosition = state.ScreenSpacePosition, Timestamp = timestamp }, state.ScreenSpacePosition, static ( handler, pos ) => handler.ReceivesPositionalInputAt( pos ) );

		if ( handler != Hovered ) {
			Hovered?.TriggerEventOnSelf( new CursorExitedEvent { EventPosition = state.ScreenSpacePosition, Timestamp = timestamp } );
			Hovered = handler;
			Hovered?.TriggerEventOnSelf( new CursorEnteredEvent { EventPosition = state.ScreenSpacePosition, Timestamp = timestamp } );
		}

		if ( Dragged == null ) {
			foreach ( var (button, (i, pos)) in pressHandlers ) { // TODO this might spam stuff with DragStartedEvents when they refuse to start a drag
				if ( (state.ScreenSpacePosition - pos).LengthSquared > DragDeadzone * DragDeadzone && i.TriggerEventOnSelf( new DragStartedEvent { Button = button, Timestamp = timestamp, EventPosition = pos } ) ) {
					Dragged = i;
					dragButton = button;
					dragStartPosition = pos;
					break;
				}
			}
		}

		if ( Dragged != null ) {
			Dragged.TriggerEventOnSelf( new DraggedEvent { Button = dragButton, Timestamp = timestamp, EventPosition = state.ScreenSpacePosition, EventStartPosition = dragStartPosition, LastEventPosition = state.LastScreenSpacePosition } );
		}
	}
}
