using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Animations;
using Vit.Framework.Input.Events;
using Vit.Framework.TwoD.Input.Events;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.TwoD.UI.Input;

public class Button : LayoutContainer, IEventHandler<HoveredEvent>, IEventHandler<PressedEvent>, IEventHandler<ReleasedEvent>, IEventHandler<ClickedEvent> {
	Box background;

	public Button () {
		AddChild( background = new Box { Tint = ColorRgba.GreenYellow }, new() {
			Size = new( 1f.Relative() )
		} );
	}

	public Action? Clicked;

	public bool OnEvent ( HoveredEvent @event ) {
		return true;
	}

	public bool OnEvent ( PressedEvent @event ) {
		if ( @event.Button != Framework.Input.CursorButton.Left )
			return false;

		background.Animate().FadeColour( ColorRgba.YellowGreen, 200 );
		return true;
	}

	public bool OnEvent ( ReleasedEvent @event ) {
		background.Animate().FadeColour( ColorRgba.GreenYellow, 200 );
		return true;
	}

	public bool OnEvent ( ClickedEvent @event ) {
		background.Animate().FlashColour( ColorRgba.White, ColorRgba.GreenYellow, 200 );
		Clicked?.Invoke();
		return true;
	}
}
