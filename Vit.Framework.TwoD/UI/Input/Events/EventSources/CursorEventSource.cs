using Vit.Framework.Input;
using Vit.Framework.Input.Events;
using Vit.Framework.Input.Trackers;
using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.UI.Input.Events.EventSources;

public class CursorEventSource<THandler> where THandler : class, IHasEventTrees<THandler>, ICanReceivePositionalInput {
	public required THandler Root { get; init; }
	Dictionary<CursorButton, THandler> buttonHandlers = new();

	public THandler? Hovered { get; private set; }

	public bool Press ( CursorState state, CursorButton button, Millis timestamp ) {
		Release( state, button, timestamp );

		if ( Hovered == null || !Hovered.TriggerEventOnSelf( new PressedEvent { Button = button, EventPosition = state.ScreenSpacePosition, Timestamp = timestamp } ) )
			return false;

		buttonHandlers[button] = Hovered;
		return true;
	}

	public bool Release ( CursorState state, CursorButton button, Millis timestamp, Action<THandler>? clicked = null ) {
		if ( !buttonHandlers.Remove( button, out var previousHandler ) )
			return false;

		bool result = previousHandler.TriggerEventOnSelf( new ReleasedEvent { Button = button, EventPosition = state.ScreenSpacePosition, Timestamp = timestamp } );
		if ( previousHandler == Hovered ) if ( previousHandler.TriggerEventOnSelf( new ClickedEvent { Button = button, EventPosition = state.ScreenSpacePosition, Timestamp = timestamp } ) ) clicked?.Invoke( previousHandler );

		return result;
	}

	public bool Move ( CursorState state, Millis timestamp ) { // TODO could we reuse/pool events? making one essentially every frame sounds bad
		var handler = Root.TriggerCulledEvent( new HoveredEvent { EventPosition = state.ScreenSpacePosition, Timestamp = timestamp }, state.ScreenSpacePosition, static ( handler, pos ) => handler.ReceivesPositionalInputAt( pos ) );

		if ( handler != Hovered ) {
			Hovered?.TriggerEventOnSelf( new CursorExitedEvent { EventPosition = state.ScreenSpacePosition, Timestamp = timestamp } );
			Hovered = handler;
			Hovered?.TriggerEventOnSelf( new CursorEnteredEvent { EventPosition = state.ScreenSpacePosition, Timestamp = timestamp } );
		}

		return Hovered != null;
	}
}
