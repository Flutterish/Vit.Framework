using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Animations;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Input;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.Tests.VisualTests;

public class LayoutAnimationTest : TestScene {
	public LayoutAnimationTest () {
		Box box;
		AddChild( box = new() { Tint = ColorRgba.Green }, new() {
			Size = (200, 200),
			Anchor = Anchor.TopLeft,
			Origin = Anchor.TopLeft
		} );

		foreach ( var anchor in new[] { Anchor.TopLeft, Anchor.BottomLeft, Anchor.BottomRight, Anchor.TopRight } ) {
			AddChild( new BasicButton {
				Clicked = () => this.Animate().ChangeLayoutParameters( box, (LayoutParams v) => v with { 
					Anchor = anchor,
					Origin = anchor
				}, 2000, Easing.InOut )
			}, new() {
				Size = (100, 100),
				Anchor = anchor,
				Origin = anchor
			} );
		}
	}
}
