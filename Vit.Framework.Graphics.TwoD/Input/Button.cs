using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Graphics.TwoD.Input.Events;
using Vit.Framework.Graphics.TwoD.Layout;

namespace Vit.Framework.Graphics.TwoD.Input;

public class Button : LayoutContainer {
	Sprite background;

	public Button () {
		AddChild( background = new Sprite { Tint = ColorRgba.GreenYellow }, new() {
			Size = new( 1f.Relative() )
		} );

		AddEventHandler<HoveredEvent>( e => true );
		AddEventHandler<PressedEvent>( e => {
			if ( e.Button != Framework.Input.CursorButton.Left )
				return false;

			background.Tint = ColorRgba.YellowGreen;
			return true;
		} );
		AddEventHandler<ReleasedEvent>( e => {
			background.Tint = ColorRgba.GreenYellow;
			return true;
		} );
		AddEventHandler<ClickedEvent>( e => {
			Clicked?.Invoke();
			return true;
		} );
	}

	public Action? Clicked;
}
