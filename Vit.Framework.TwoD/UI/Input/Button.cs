using Vit.Framework.Graphics;
using Vit.Framework.Input.Events;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Input.Events;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.TwoD.UI.Input;

public class Button : LayoutContainer, IEventHandler<HoveredEvent>, IEventHandler<PressedEvent>, IEventHandler<ReleasedEvent>, IEventHandler<ClickedEvent> {
	Sprite background;

	public Button () {
		AddChild( background = new Sprite { Tint = ColorRgba.GreenYellow }, new() {
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

		background.Tint = ColorRgba.YellowGreen;
		return true;
	}

	public bool OnEvent ( ReleasedEvent @event ) {
		background.Tint = ColorRgba.GreenYellow;
		return true;
	}

	public bool OnEvent ( ClickedEvent @event ) {
		Clicked?.Invoke();
		return true;
	}
}
