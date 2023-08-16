using Vit.Framework.Input;
using Vit.Framework.Input.Events;
using Vit.Framework.Input.Trackers;

namespace Vit.Framework.TwoD.Input.Events.EventSources;

public class CursorEventSource<THandler> where THandler : class, IHasEventTrees<THandler>, ICanReceivePositionalInput {
	public required THandler Root { get; init; }
	Dictionary<CursorButton, THandler> buttonHandlers = new();

	public THandler? Hovered { get; private set; }

	public bool Press ( CursorState state, CursorButton button ) {
		Release( state, button );

		if ( Hovered == null || !Hovered.TriggerEventOnSelf( new PressedEvent { Button = button, EventPosition = state.ScreenSpacePosition } ) )
			return false;

		buttonHandlers[button] = Hovered;
		return true;
	}

	public bool Release ( CursorState state, CursorButton button, Action<THandler>? clicked = null ) {
		if ( !buttonHandlers.Remove( button, out var previousHandler ) )
			return false;

		bool result = previousHandler.TriggerEventOnSelf( new ReleasedEvent { Button = button, EventPosition = state.ScreenSpacePosition } );
		if ( previousHandler == Hovered ) {
			if( previousHandler.TriggerEventOnSelf( new ClickedEvent { Button = button, EventPosition = state.ScreenSpacePosition } ) ) {
				clicked?.Invoke( previousHandler );
			}
		}

		return result;
	}

	public bool Move ( CursorState state ) { // TODO could we reuse/pool events? making one essentially every frame sounds bad
		var handler = Root.TriggerCulledEvent( new HoveredEvent { EventPosition = state.ScreenSpacePosition }, state.ScreenSpacePosition, static (handler, pos) => handler.ReceivesPositionalInputAt( pos ) );

		if ( handler != Hovered ) {
			Hovered?.TriggerEventOnSelf( new CursorExitedEvent { EventPosition = state.ScreenSpacePosition } );
			Hovered = handler;
			Hovered?.TriggerEventOnSelf( new CursorEnteredEvent { EventPosition = state.ScreenSpacePosition } );
		}
		
		return Hovered != null;
	}
}
