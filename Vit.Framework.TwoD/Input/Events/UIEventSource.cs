﻿using Vit.Framework.Input;
using Vit.Framework.Input.Events;
using Vit.Framework.TwoD.UI;

namespace Vit.Framework.TwoD.Input.Events;

public class UIEventSource {
	public required UIComponent Root { get; init; }

	Dictionary<CursorButton, UIComponent> pressedHandlers = new();
	UIComponent? hovered;

	/// <summary>
	/// Triggers UI events based on the provided events.
	/// </summary>
	/// <returns><see langword="true"/> if the event was translated to one or more UI events, <see langword="false"/> otherwise.</returns>
	public bool TriggerEvent ( Event @event ) {
		UIComponent? handler = null;
		switch ( @event ) {
			case CursorButtonPressedEvent pressed:
				if ( hovered == null )
					break;

				if ( triggerEvent( new PressedEvent { Button = pressed.Button, EventPosition = pressed.EventPosition, Timestamp = pressed.Timestamp }, hovered ) )
					pressedHandlers.Add( pressed.Button, hovered );
				break;

			case CursorButtonReleasedEvent released:
				if ( !pressedHandlers.TryGetValue( released.Button, out handler ) )
					break;
				pressedHandlers.Remove( released.Button );
				triggerEvent( new ReleasedEvent { Button = released.Button, EventPosition = released.EventPosition, Timestamp = released.Timestamp }, handler );
				if ( handler == hovered ) triggerEvent( new ClickedEvent { Button = released.Button, EventPosition = released.EventPosition, Timestamp = released.Timestamp }, handler );
				break;

			case CursorMovedEvent moved:
				handler = triggerEvent( new HoveredEvent { EventPosition = moved.EventPosition, Timestamp = moved.Timestamp } );
				if ( handler == hovered )
					break;

				triggerEvent( new CursorExitedEvent { EventPosition = moved.EventPosition, Timestamp = moved.Timestamp }, hovered );
				hovered = handler;
				triggerEvent( new CursorEnteredEvent { EventPosition = moved.EventPosition, Timestamp = moved.Timestamp }, handler );
				break;

			default:
				return false;
		}

		return true;
	}

	UIComponent? triggerEvent ( Event e ) {
		var handler = e is IPositionalEvent positional
					? Root.TriggerCulledEvent( e, positional.EventPosition, static ( d, pos ) => d.ReceivesPositionalInputAt( pos ) )
					: Root.TriggerEvent( e );

		if ( e is ILoggableEvent ) Console.WriteLine( $"{e} was {(handler is null ? "not handled" : $"handled by {handler}")}" );

		return handler;
	}

	bool triggerEvent ( Event e, UIComponent? handler ) {
		if ( handler == null )
			return false;

		var handled = handler.TriggerEventOnSelf( e );

		if ( e is ILoggableEvent ) Console.WriteLine( $"{e} was trigerred on {handler} {(handled ? "and handled" : "but not handled")}" );
		return handled;
	}
}