using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Graphics.TwoD.Input.Events;
using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Input.Events;

namespace Vit.Framework.Graphics.TwoD.Input;

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
