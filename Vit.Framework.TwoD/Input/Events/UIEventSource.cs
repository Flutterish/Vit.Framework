﻿using Vit.Framework.Input;
using Vit.Framework.Input.Events;
using Vit.Framework.TwoD.UI;

namespace Vit.Framework.TwoD.Input.Events;

public class UIEventSource {
	public required UIComponent Root { get; init; }

	Dictionary<CursorButton, UIComponent> cursorHandlers = new();
	Dictionary<Key, UIComponent> keyboardHandlers = new();
	UIComponent? hovered;

	/// <summary>
	/// Triggers UI events based on the provided events.
	/// </summary>
	/// <returns><see langword="true"/> if the event was translated to one or more UI events, <see langword="false"/> otherwise.</returns>
	public bool TriggerEvent ( Event @event ) { // TODO also keep track of the source. this will be important when we get around to touch input
		UIComponent? handler = null;
		switch ( @event ) {
			case CursorButtonPressedEvent pressed:
				if ( hovered == null )
					break;

				if ( cursorHandlers.Remove( pressed.Button, out var previousHandler ) )
					triggerEvent( new ReleasedEvent { Button = pressed.Button, EventPosition = pressed.EventPosition }, previousHandler );

				if ( triggerEvent( new PressedEvent { Button = pressed.Button, EventPosition = pressed.EventPosition }, hovered ) )
					cursorHandlers.Add( pressed.Button, hovered );
				break;

			case CursorButtonReleasedEvent released:
				if ( !cursorHandlers.Remove( released.Button, out handler ) )
					break;

				triggerEvent( new ReleasedEvent { Button = released.Button, EventPosition = released.EventPosition }, handler );
				if ( handler == hovered ) 
					triggerEvent( new ClickedEvent { Button = released.Button, EventPosition = released.EventPosition }, handler );
				break;

			case CursorMovedEvent moved:
				handler = triggerEvent( new HoveredEvent { EventPosition = moved.EventPosition } ); // TODO could we reuse/pool events? making one essentially every frame sounds bad
				if ( handler == hovered )
					break;

				triggerEvent( new CursorExitedEvent { EventPosition = moved.EventPosition }, hovered );
				hovered = handler;
				triggerEvent( new CursorEnteredEvent { EventPosition = moved.EventPosition }, handler );
				break;

			case TextInputEvent text: // TODO this should be fed directly into a focused element
				triggerEvent( new UITextInputEvent { Text = text.Text } );
				break;

			case KeyDownEvent down:
				if ( keyboardHandlers.TryGetValue( down.Key, out handler ) )
					triggerEvent( new KeyUpEvent<Key> { Key = down.Key }, handler );

				handler = triggerEvent( new KeyDownEvent<Key> { Key = down.Key } );
				if ( handler != null )
					keyboardHandlers.Add( down.Key, handler );
				break;

			case KeyUpEvent up:
				if ( keyboardHandlers.Remove( up.Key, out handler ) )
					triggerEvent( new KeyUpEvent<Key> { Key = up.Key }, handler );
				break;

			case KeyRepeatEvent repeat:
				if ( keyboardHandlers.TryGetValue( repeat.Key, out handler ) )
					triggerEvent( new KeyRepeatEvent<Key> { Key = repeat.Key }, handler );
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
