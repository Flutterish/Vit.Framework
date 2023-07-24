using Vit.Framework.Input;
using Vit.Framework.Input.Events;

namespace Vit.Framework.Graphics.TwoD.Input.Events;

public class UIEventSource {
	public required IDrawable Root { get; init; }

	Dictionary<CursorButton, IDrawable> pressedHandlers = new();

	/// <summary>
	/// Triggers UI events based on the provided events.
	/// </summary>
	/// <returns><see langword="true"/> if the event was translated to one or more UI events, <see langword="false"/> otherwise.</returns>
	public bool TriggerEvent ( Event @event ) {
		IDrawable? handler = null;
		switch ( @event ) {
			case CursorButtonPressedEvent pressed:
				@event = new PressedEvent { Button = pressed.Button, EventPosition = pressed.EventPosition, Timestamp = pressed.Timestamp };
				handler = triggerEvent( @event );
				if ( handler != null )
					pressedHandlers.Add( pressed.Button, handler );
				break;

			case CursorButtonReleasedEvent released:
				if ( !pressedHandlers.TryGetValue( released.Button, out handler ) )
					break;
				@event = new ReleasedEvent { Button = released.Button, EventPosition = released.EventPosition, Timestamp = released.Timestamp };
				pressedHandlers.Remove( released.Button );
				triggerEvent( @event, handler );
				if ( handler.ReceivesPositionalInputAt( released.EventPosition ) ) {
					@event = new ClickedEvent { Button = released.Button, EventPosition = released.EventPosition, Timestamp = released.Timestamp };
					triggerEvent( @event, handler );
				}
				break;

			default:
				return false;
		}

		return true;
	}

	IDrawable? triggerEvent ( Event e ) {
		var handler = e is IPositionalEvent positional
					? Root.TriggerCulledEvent( e, positional.EventPosition, static (d, pos) => d.ReceivesPositionalInputAt( pos ) )
					: Root.TriggerEvent( e );

		logHandler( e, handler );
		return handler;
	}

	IDrawable? triggerEvent ( Event e, IDrawable handler ) {
		var handled = handler.TriggerEventOnSelf( e );

		logHandler( e, handled ? handler : null );
		return handler;
	}

	void logHandler ( Event e, IDrawable? handler ) {
		if ( e is not ILoggableEvent )
			return;

		Console.WriteLine( $"{e} was {(handler is null ? "not handled" : $"handled by {handler}")}" );
	}
}
