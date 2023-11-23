using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Animations;
using Vit.Framework.Localisation;
using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Animations;
using Vit.Framework.TwoD.UI.Graphics;

namespace Vit.Framework.TwoD.UI.Input;

public class BasicButton : Button {
	ColorRgb<float> hoverColour = FrameworkUIScheme.ElementHovered;
	ColorRgb<float> backgroundColour = FrameworkUIScheme.Element;
	ColorRgb<float> pressedColour = FrameworkUIScheme.ElementPressed;
	ColorRgb<float> flashColour = FrameworkUIScheme.ElementFlash;
	public ColorRgb<float> HoverColour {
		get => hoverColour;
		set {
			if ( value.TrySet( ref hoverColour ) )
				OnStateChanged( State );
		}
	}
	public ColorRgb<float> BackgroundColour {
		get => backgroundColour;
		set {
			if ( value.TrySet( ref backgroundColour ) )
				OnStateChanged( State );
		}
	}
	public ColorRgb<float> PressedColour {
		get => pressedColour;
		set {
			if ( value.TrySet( ref pressedColour ) )
				OnStateChanged( State );
		}
	}
	public ColorRgb<float> FlashColour {
		get => flashColour;
		set {
			if ( value.TrySet( ref flashColour ) )
				OnStateChanged( State );
		}
	}

	Box background;
	public readonly SpriteText SpriteText;
	public LocalisableString Text {
		get => SpriteText.Text;
		set => SpriteText.Text = value;
	}
	public string RawText {
		get => SpriteText.RawText;
		set => SpriteText.RawText = value;
	}
	public RelativeAxes2<float> TextAnchor {
		get => GetLayoutParameters( SpriteText ).Anchor;
		set => UpdateLayoutParameters( SpriteText, value, ( v, value ) => v with { Anchor = value } );
	}
	public RelativeAxes2<float> TextOrigin {
		get => GetLayoutParameters( SpriteText ).Origin;
		set => UpdateLayoutParameters( SpriteText, value, ( v, value ) => v with { Origin = value } );
	}
	public BasicButton () {
		AddChild( background = new Box { Tint = BackgroundColour }, new() {
			Size = new( 1f.Relative() )
		} );
		AddChild( SpriteText = new() { FontIdentifier = FrameworkUIScheme.FontCollection, FontSize = 32 }, new() {
			Anchor = Anchor.CentreLeft,
			Origin = Anchor.CentreLeft
		} );
	}

	protected override void OnStateChanged ( ButtonState state ) {
		switch ( state ) {
			case ButtonState.Disabled:
				break;

			case ButtonState.Neutral:
				background.Animate().FadeColour( BackgroundColour, 200.Millis(), Easing.Out );
				break;

			case ButtonState.Hovered:
				background.Animate().FadeColour( HoverColour, 300.Millis() );
				break;

			case ButtonState.HeldDown:
				background.Animate().FadeColour( PressedColour, 300.Millis(), Easing.Out );
				break;
		}
	}

	protected override void OnClicked () {
		background.Animate().FlashColour( FlashColour, State == ButtonState.Neutral ? BackgroundColour : HoverColour, 200.Millis() );
	}
}
