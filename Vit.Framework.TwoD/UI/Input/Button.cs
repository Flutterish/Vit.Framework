using Vit.Framework.DependencyInjection;
using Vit.Framework.Input;
using Vit.Framework.TwoD.UI.Input.Events;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.TwoD.UI.Input;

public abstract class Button : LayoutContainer, IClickable, IHoverable, ITabbable, IKeyBindingHandler<Key> {
	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		base.OnLoad( dependencies );
		updateState();
	}

	protected ButtonState State { get; private set; }
	protected abstract void OnStateChanged ( ButtonState state );
	protected abstract void OnClicked ();

	Action? clicked;
	public Action? Clicked {
		get => clicked;
		set {
			clicked = value;
			if ( IsLoaded )
				updateState();
		}
	}
	bool isHovered;
	bool isPressed;

	void updateState () {
		var newState =
			Clicked == null ? ButtonState.Disabled
			: isPressed ? ButtonState.HeldDown
			: isHovered ? ButtonState.Hovered
			: ButtonState.Neutral;

		if ( newState == State )
			return;

		State = newState;
		OnStateChanged( State );
	}

	public bool OnPressed ( PressedEvent @event ) {
		if ( Clicked == null || @event.Button != CursorButton.Left )
			return false;

		isPressed = true;
		updateState();
		return true;
	}

	public bool OnReleased ( ReleasedEvent @event ) {
		isPressed = false;
		updateState();
		return true;
	}

	public bool OnClicked ( ClickedEvent @event ) {
		OnClicked();
		Clicked?.Invoke();
		return true;
	}

	public bool OnHovered ( HoveredEvent @event ) {
		return true;
	}

	public bool OnCursorEntered ( CursorEnteredEvent @event ) {
		isHovered = true;
		updateState();
		return true;
	}

	public bool OnCursorExited ( CursorExitedEvent @event ) {
		isHovered = false;
		updateState();
		return true;
	}

	Focus? focus;
	public bool OnFocused ( FocusGainedEvent @event ) {
		focus = @event.Focus;
		return true;
	}

	public bool OnFocusLost ( FocusLostEvent @event ) {
		focus = null;
		return true;
	}

	public bool OnTabbedOver ( TabbedOverEvent @event ) {
		return Clicked != null;
	}

	protected override void OnUnload () {
		base.OnUnload();
		focus?.Release();
	}

	public bool OnKeyDown ( Key key, bool isRepeat ) {
		if ( Clicked == null || focus == null )
			return false;

		if ( key is Key.Enter or Key.Space ) {
			OnClicked();
			Clicked?.Invoke();

			return true;
		}
		return false;
	}

	public bool OnKeyUp ( Key key ) {
		return true;
	}
}

public enum ButtonState {
	Disabled,
	Neutral,
	Hovered,
	HeldDown
}