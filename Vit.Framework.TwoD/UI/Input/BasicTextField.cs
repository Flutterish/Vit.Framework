using Vit.Framework.Graphics;
using Vit.Framework.Input;
using Vit.Framework.Input.Events;
using Vit.Framework.TwoD.Input.Events;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.TwoD.UI.Input;

public class BasicTextField : LayoutContainer, IEventHandler<UITextInputEvent>, IKeyBindingHandler<Key> {
	SpriteText spriteText;
	public string Text {
		get => spriteText.Text;
		set => spriteText.Text = value;
	}
	public BasicTextField () {
		AutoSizeDirection = LayoutDirection.Both;
		AddChild( new Box() { Tint = ColorRgba.YellowGreen }, new() {
			Size = new(1f.Relative())
		} );
		AddChild( spriteText = new() { FontSize = 32 }, new() {
			Size = (20, 0),
			Anchor = Anchor.CentreLeft,
			Origin = Anchor.CentreLeft
		} );
	}

	public bool OnEvent ( UITextInputEvent @event ) {
		Text += @event.Text;
		return true;
	}

	public bool OnKeyDown ( Key key, bool isRepeat ) {
		switch ( key ) {
			case Key.Backspace:
				if ( Text != "" )
					Text = Text.Substring( 0, Text.Length - 1 );
				return true;

			default:
				return false;
		}
	}

	public bool OnKeyUp ( Key key ) {
		return false;
	}
}
