using Vit.Framework.DependencyInjection;
using Vit.Framework.Input;
using Vit.Framework.Input.Events;
using Vit.Framework.TwoD.UI;
using Vit.Framework.TwoD.UI.Input;

namespace Vit.Framework.TwoD.Input.Events;

public class UIEventSource {
	public required UIComponent Root { get; init; }

	PlatformActionBindings platformBindings = new DefaultPlatformActionBindings();
	Dictionary<CursorButton, UIComponent> cursorHandlers = new();
	Dictionary<Key, UIComponent> keyboardHandlers = new();
	Dictionary<PlatformAction, UIComponent> platformActionHandlers = new();
	UIComponent? hovered;
	UIComponent? focused;
	UIFocus focus;
	Clipboard clipboard;

	EventTree<UIComponent>? lastValidTabIndex;
	EventTree<UIComponent>? currentTabIndex;
	public BasicTabVisualizer? TabVisualizer { get; init; }
	bool isTabFocused;

	public UIEventSource ( IReadOnlyDependencyCache dependencies ) {
		clipboard = dependencies.Resolve<Clipboard>();
		platformBindings.Pressed += onPressed;
		platformBindings.Repeated += onRepeated;
		platformBindings.Released += onReleased;

		focus = new( this );
	}

	bool pressKey<TKey> ( Dictionary<TKey, UIComponent> map, TKey value, Action? releasedPrevious = null ) where TKey : struct, Enum {
		if ( map.TryGetValue( value, out var handler ) ) {
			triggerEvent( new KeyUpEvent<TKey> { Key = value }, handler );
			releasedPrevious?.Invoke();
		}

		var @event = new KeyDownEvent<TKey> { Key = value };
		if ( triggerEvent( @event, focused ) ) {
			map.Add( value, focused! );
			return true;
		}
		else {
			handler = triggerEvent( @event );
			if ( handler != null ) {
				map.Add( value, handler );
				return true;
			}
		}

		return false;
	}

	void releaseKey<TKey> ( Dictionary<TKey, UIComponent> map, TKey value ) where TKey : struct, Enum {
		if ( map.Remove( value, out var handler ) )
			triggerEvent( new KeyUpEvent<TKey> { Key = value }, handler );
	}

	bool repeatKey<TKey> ( Dictionary<TKey, UIComponent> map, TKey value ) where TKey : struct, Enum {
		if ( map.TryGetValue( value, out var handler ) )
			return triggerEvent( new KeyRepeatEvent<TKey> { Key = value }, handler );

		return false;
	}

	void onReleased ( PlatformAction action ) {
		releaseKey( platformActionHandlers, action );
	}

	void onRepeated ( PlatformAction action ) {
		if ( repeatKey( platformActionHandlers, action ) )
			return;

		onPlatformAction( action );
	}

	void onPressed ( PlatformAction action ) {
		if ( pressKey( platformActionHandlers, action ) )
			return;

		onPlatformAction( action );
	}
	
	void onPlatformAction ( PlatformAction action ) {
		if ( action == PlatformAction.Copy ) {
			triggerEvent( new ClipboardCopyEvent { Clipboard = clipboard }, focused );
		}
		else if ( action == PlatformAction.Paste ) {
			if ( clipboard.GetText( 0 ) is string text )
				triggerEvent( new ClipboardPasteTextEvent { Clipboard = clipboard, Text = text }, focused );
		}
		else if ( action == PlatformAction.TabForward ) {
			tab( forward: true );
		}
		else if ( action == PlatformAction.TabBackward ) {
			tab( forward: false );
		}
	}

	void tab ( bool forward ) {
		var previous = currentTabIndex;
		if ( currentTabIndex == null ) {
			if ( forward ) {
				currentTabIndex = Root.HandledEventTypes.TryGetValue( typeof( TabFocusGainedEvent ), out var index ) ? index : null;
				if ( currentTabIndex?.Handler == null )
					currentTabIndex = currentTabIndex?.NextWithHandler;
			}
			else {
				currentTabIndex = lastValidTabIndex;
			}
		}
		else {
			currentTabIndex = forward ? currentTabIndex.NextWithHandler : currentTabIndex.PreviousWithHandler;
		}

		releaseFocus();
		TabFocusGainedEvent @event = new() { Focus = focus };
		while ( currentTabIndex != null ) {
			if ( currentTabIndex.Handler!.Invoke( @event ) ) {
				focused = currentTabIndex.Source;
				isTabFocused = true;
				if ( TabVisualizer != null )
					TabVisualizer.Target = focused;
				return;
			}

			previous = currentTabIndex;
			currentTabIndex = forward ? currentTabIndex.NextWithHandler : currentTabIndex.PreviousWithHandler;
		}

		if ( forward )
			lastValidTabIndex = previous;
		else
			lastValidTabIndex = null;
	}

	void releaseFocus () {
		isTabFocused = false;
		if ( TabVisualizer != null )
			TabVisualizer.Target = null;
		triggerEvent( new FocusLostEvent { Focus = focus }, focused );
		focus = new( this );
		focused = null;
	}

	void manualReleaseFocus () {
		releaseFocus();
		currentTabIndex = null;
		lastValidTabIndex = null;
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
				
				if ( hovered != focused && focused != null ) {
					releaseFocus();
					lastValidTabIndex = null;
					currentTabIndex = hovered.HandledEventTypes.TryGetValue( typeof( TabFocusGainedEvent ), out var tabbable ) ? tabbable : null; 
				}

				if ( triggerEvent( new PressedEvent { Button = pressed.Button, EventPosition = pressed.EventPosition }, hovered ) )
					cursorHandlers.Add( pressed.Button, hovered );
				break;

			case CursorButtonReleasedEvent released:
				if ( !cursorHandlers.Remove( released.Button, out handler ) )
					break;

				triggerEvent( new ReleasedEvent { Button = released.Button, EventPosition = released.EventPosition }, handler );
				if ( handler == hovered ) {
					triggerEvent( new ClickedEvent { Button = released.Button, EventPosition = released.EventPosition }, handler );
					if ( focused == handler )
						break;

					if ( focused != null )
						releaseFocus();
					if ( triggerEvent( new FocusGainedEvent { Focus = focus }, handler ) )
						focused = handler;
				}
				break;

			case CursorMovedEvent moved:
				handler = triggerEvent( new HoveredEvent { EventPosition = moved.EventPosition } ); // TODO could we reuse/pool events? making one essentially every frame sounds bad
				if ( handler == hovered )
					break;

				triggerEvent( new CursorExitedEvent { EventPosition = moved.EventPosition }, hovered );
				hovered = handler;
				triggerEvent( new CursorEnteredEvent { EventPosition = moved.EventPosition }, handler );
				break;

			case TextInputEvent text:
				if ( focused != null )
					triggerEvent( new UITextInputEvent { Text = text.Text }, focused );
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


	class UIFocus : Focus {
		UIEventSource source;

		public UIFocus ( UIEventSource source ) {
			this.source = source;
		}

		public override void Release () {
			if ( source.focus == this )
				source.manualReleaseFocus();
		}
	}
}
