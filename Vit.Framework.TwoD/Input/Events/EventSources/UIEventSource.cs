using Vit.Framework.DependencyInjection;
using Vit.Framework.Input;
using Vit.Framework.Input.Events;
using Vit.Framework.Timing;
using Vit.Framework.TwoD.UI;
using Vit.Framework.TwoD.UI.Input;

namespace Vit.Framework.TwoD.Input.Events.EventSources;

public class UIEventSource {
	public UIComponent Root { get; private init; }

	CursorEventSource<UIComponent> cursorEvents;

	PlatformActionBindings platformBindings = new DefaultPlatformActionBindings();
	KeyBindingEventSource<PlatformAction, UIComponent> platformActionEvents;
	KeyBindingEventSource<Key, UIComponent> keyboardEvents;

	public BasicTabVisualizer TabVisualizer { get; }
	TabableFocusSource<UIComponent> tabFocus;
	public readonly UIFocus Focus;
	UIComponent? focused;

	Clipboard clipboard;

	IClock clock;
	public UIEventSource ( UIComponent root, IReadOnlyDependencyCache dependencies ) {
		Root = root;
		clipboard = dependencies.Resolve<Clipboard>();
		clock = dependencies.Resolve<IClock>();

		platformBindings.Pressed += onPressed;
		platformBindings.Repeated += onRepeated;
		platformBindings.Released += onReleased;

		Focus = new( this );

		keyboardEvents = new() { Root = Root };
		platformActionEvents = new() { Root = Root };
		cursorEvents = new() { Root = Root };
		tabFocus = new() { Root = Root };

		TabVisualizer = new();
	}

	void onReleased ( PlatformAction action ) {
		platformActionEvents.Release( action, eventTimestamp );
	}

	void onRepeated ( PlatformAction action ) {
		if ( platformActionEvents.Repeat( action, eventTimestamp ) )
			return;

		onPlatformAction( action );
	}

	void onPressed ( PlatformAction action ) {
		if ( focused != null && platformActionEvents.Press( action, eventTimestamp, focused ) )
			return;

		onPlatformAction( action );
	}

	double eventTimestamp;
	void onPlatformAction ( PlatformAction action ) {
		if ( action == PlatformAction.Copy ) {
			focused?.TriggerEventOnSelf( new ClipboardCopyEvent { Clipboard = clipboard, Timestamp = eventTimestamp } );
		}
		else if ( action == PlatformAction.Cut ) {
			focused?.TriggerEventOnSelf( new ClipboardCutEvent { Clipboard = clipboard, Timestamp = eventTimestamp } );
		}
		else if ( action == PlatformAction.Paste ) {
			if ( clipboard.GetText( 0 ) is string text )
				focused?.TriggerEventOnSelf( new ClipboardPasteTextEvent { Clipboard = clipboard, Text = text, Timestamp = eventTimestamp } );
		}
		else if ( action == PlatformAction.TabForward ) {
			setFocus( tabFocus.TabForward( eventTimestamp ), byTab: true );
		}
		else if ( action == PlatformAction.TabBackward ) {
			setFocus( tabFocus.TabBackward( eventTimestamp ), byTab: true );
		}
	}

	void setFocus ( UIComponent? target, bool byTab = false ) {
		if ( byTab ) {
			TabVisualizer.Target = target;
		}
		else {
			tabFocus.ReleaseTabFocus();
			TabVisualizer.Target = null;
		}

		if ( focused == target )
			return;

		focused?.TriggerEventOnSelf( new FocusLostEvent { Focus = Focus, Timestamp = eventTimestamp } );
		focused = target;
		focused?.TriggerEventOnSelf( new FocusGainedEvent { Focus = Focus, Timestamp = eventTimestamp } );
	}

	/// <summary>
	/// Triggers UI events based on the provided events.
	/// </summary>
	/// <returns><see langword="true"/> if the event was translated to one or more UI events, <see langword="false"/> otherwise.</returns>
	public bool TriggerEvent ( TimestampedEvent @event ) { // TODO also keep track of the source. this will be important when we get around to touch input
		eventTimestamp = (@event.Timestamp - clock.ClockEpoch).TotalMilliseconds;

		switch ( @event ) {
			case CursorButtonPressedEvent pressed:
				if ( cursorEvents.Hovered != focused ) {
					setFocus( null );
					tabFocus.FindClosestTabIndex( cursorEvents.Hovered );
				}

				cursorEvents.Press( pressed.CursorState, pressed.Button, eventTimestamp );
				break;

			case CursorButtonReleasedEvent released:
				cursorEvents.Release( released.CursorState, released.Button, eventTimestamp, clicked: handler => {
					setFocus( handler );
				} );
				break;

			case CursorMovedEvent moved:
				cursorEvents.Move( moved.CursorState, eventTimestamp );
				break;

			case TextInputEvent text:
				focused?.TriggerEventOnSelf( new UITextInputEvent { Text = text.Text, Timestamp = eventTimestamp } );
				break;

			case KeyDownEvent down:
				if ( focused != null )
					keyboardEvents.Press( down.Key, eventTimestamp, focused );
				platformBindings.Add( down.Key );
				break;

			case KeyUpEvent up:
				keyboardEvents.Release( up.Key, eventTimestamp );
				platformBindings.Remove( up.Key );
				break;

			case KeyRepeatEvent repeat:
				keyboardEvents.Repeat( repeat.Key, eventTimestamp );
				platformBindings.Repeat( repeat.Key );
				break;

			default:
				return false;
		}

		return true;
	}

	public class UIFocus : Focus {
		UIEventSource source;
		public UIComponent? Target => source.focused;
	
		public UIFocus ( UIEventSource source ) {
			this.source = source;
		}
	
		public override void Release () {
			source.setFocus( null );
		}
	}
}
