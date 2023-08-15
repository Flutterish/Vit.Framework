using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Animations;
using Vit.Framework.Input;
using Vit.Framework.TwoD.Input.Events;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Animations;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.TwoD.UI.Input;

public class BasicButton : LayoutContainer, IClickable, IHoverable, ITabbable, IKeyBindingHandler<Key> {
	ColorRgba<float> hoverColour = ColorRgba.YellowGreen.Interpolate( ColorRgba.GreenYellow, 0.5f );
	ColorRgba<float> backgroundColour = ColorRgba.GreenYellow;
	ColorRgba<float> pressedColour = ColorRgba.YellowGreen.Interpolate( ColorRgba.Black, 0.1f );
	ColorRgba<float> flashColour = ColorRgba.White;
	public ColorRgba<float> HoverColour {
		get => hoverColour;
		set {
			if ( value.TrySet( ref hoverColour ) )
				onInteractionStateChanged();
		}
	}
	public ColorRgba<float> BackgroundColour {
		get => backgroundColour;
		set {
			if ( value.TrySet( ref backgroundColour ) )
				onInteractionStateChanged();
		}
	}
	public ColorRgba<float> PressedColour {
		get => pressedColour;
		set {
			if ( value.TrySet( ref pressedColour ) )
				onInteractionStateChanged();
		}
	}
	public ColorRgba<float> FlashColour {
		get => flashColour;
		set {
			if ( value.TrySet( ref flashColour ) )
				onInteractionStateChanged();
		}
	}

	Box background;
	public BasicButton () {
		AddChild( background = new Box { Tint = ColorRgba.GreenYellow }, new() {
			Size = new( 1f.Relative() )
		} );
	}

	public Action? Clicked;
	bool isHovered;
	bool isPressed;

	void onInteractionStateChanged () {
		if ( isPressed )
			background.Animate().FadeColour( PressedColour, 300, Easing.Out );
		else if ( isHovered )
			background.Animate().FadeColour( HoverColour, 300 );
		else
			background.Animate().FadeColour( BackgroundColour, 200, Easing.Out );
	}

	public bool OnPressed ( PressedEvent @event ) {
		if ( @event.Button != Framework.Input.CursorButton.Left )
			return false;

		isPressed = true;
		onInteractionStateChanged();
		return true;
	}

	public bool OnReleased ( ReleasedEvent @event ) {
		isPressed = false;
		onInteractionStateChanged();
		return true;
	}

	public bool OnClicked ( ClickedEvent @event ) {
		background.Animate().FlashColour( FlashColour, HoverColour, 200 );
		Clicked?.Invoke();
		return true;
	}

	public bool OnHovered ( HoveredEvent @event ) {
		return true;
	}

	public bool OnCursorEntered ( CursorEnteredEvent @event ) {
		isHovered = true;
		onInteractionStateChanged();
		return true;
	}

	public bool OnCursorExited ( CursorExitedEvent @event ) {
		isHovered = false;
		onInteractionStateChanged();
		return true;
	}

	Focus? focus;
	public bool OnTabbedOver ( TabFocusGainedEvent @event ) {
		focus = @event.Focus;
		return true;
	}

	public bool OnEvent ( FocusLostEvent @event ) {
		focus = null;
		return true;
	}

	protected override void OnUnload () {
		base.OnUnload();
		focus?.Release();
	}

	public bool OnKeyDown ( Key key, bool isRepeat ) {
		if ( focus == null )
			return false;

		if ( key is Key.Enter or Key.Space ) {
			background.Animate().FlashColour( FlashColour, isHovered ? HoverColour : BackgroundColour, 200 );
			Clicked?.Invoke();

			return true;
		}
		return false;
	}

	public bool OnKeyUp ( Key key ) {
		return true;
	}
}
