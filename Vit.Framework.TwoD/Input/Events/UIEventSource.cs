using Vit.Framework.DependencyInjection;
using Vit.Framework.Input;
using Vit.Framework.Input.Events;
using Vit.Framework.TwoD.UI;

namespace Vit.Framework.TwoD.Input.Events;

public class UIEventSource {
	public required UIComponent Root { get; init; }

	PlatformActionBindings platformBindings = new DefaultPlatformActionBindings();
	Dictionary<CursorButton, UIComponent> cursorHandlers = new();
	Dictionary<Key, UIComponent> keyboardHandlers = new();
	Dictionary<PlatformAction, UIComponent> platformActionHandlers = new();
	UIComponent? hovered;
	Clipboard clipboard;

	public UIEventSource ( IReadOnlyDependencyCache dependencies ) {
		clipboard = dependencies.Resolve<Clipboard>();
		platformBindings.Pressed += onPressed;
		platformBindings.Repeated += onRepeated;
		platformBindings.Released += onReleased;
	}

	void pressKey<TKey> ( Dictionary<TKey, UIComponent> map, TKey value, Action? releasedPrevious = null ) where TKey : struct, Enum {
		if ( map.TryGetValue( value, out var handler ) ) {
			triggerEvent( new KeyUpEvent<TKey> { Key = value }, handler );
			releasedPrevious?.Invoke();
		}

		handler = triggerEvent( new KeyDownEvent<TKey> { Key = value } );
		if ( handler != null )
			map.Add( value, handler );
	}

	void releaseKey<TKey> ( Dictionary<TKey, UIComponent> map, TKey value ) where TKey : struct, Enum {
		if ( map.Remove( value, out var handler ) )
			triggerEvent( new KeyUpEvent<TKey> { Key = value }, handler );
	}

	void repeatKey<TKey> ( Dictionary<TKey, UIComponent> map, TKey value ) where TKey : struct, Enum {
		if ( map.TryGetValue( value, out var handler ) )
			triggerEvent( new KeyRepeatEvent<TKey> { Key = value }, handler );
	}

	private void onReleased ( PlatformAction action ) {
		releaseKey( platformActionHandlers, action );
	}

	private void onRepeated ( PlatformAction action ) {
		repeatKey( platformActionHandlers, action );

		if ( action == PlatformAction.Copy ) {
			triggerEvent( new ClipboardCopyEvent { Clipboard = clipboard } );
		}
		else if ( action == PlatformAction.Paste && clipboard.GetText( 0 ) is string text ) {
			triggerEvent( new ClipboardPasteTextEvent { Clipboard = clipboard, Text = text } );
		}
	}

	private void onPressed ( PlatformAction action ) {
		pressKey( platformActionHandlers, action );

		if ( action == PlatformAction.Copy ) {
			triggerEvent( new ClipboardCopyEvent { Clipboard = clipboard } );
		}
		else if ( action == PlatformAction.Paste && clipboard.GetText( 0 ) is string text ) {
			triggerEvent( new ClipboardPasteTextEvent { Clipboard = clipboard, Text = text } );
		}
	}

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
				pressKey( keyboardHandlers, down.Key, () => platformBindings.Remove( down.Key ) );
				platformBindings.Add( down.Key );
				break;

			case KeyUpEvent up:
				releaseKey( keyboardHandlers, up.Key );
				platformBindings.Remove( up.Key );
				break;

			case KeyRepeatEvent repeat:
				repeatKey( keyboardHandlers, repeat.Key );
				platformBindings.Repeat( repeat.Key );
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
