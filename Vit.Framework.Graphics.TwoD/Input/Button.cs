using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Input.Events;

namespace Vit.Framework.Graphics.TwoD.Input;

public class Button : LayoutContainer {
	Sprite background;

	public Button () {
		AddChild( background = new Sprite { Tint = ColorRgba.GreenYellow }, new() {
			Size = new( 1f.Relative() )
		} );

		// TODO some events are "finalizers" that should activate the same handler again even if they arent the closest one
		AddEventHandler<CursorButtonPressedEvent>( e => {
			background.Tint = ColorRgba.YellowGreen;
			return true;
		} );
		AddEventHandler<CursorButtonReleasedEvent>( e => {
			background.Tint = ColorRgba.GreenYellow;
			Clicked?.Invoke();
			return true;
		} );
	}

	public Action? Clicked;
}
