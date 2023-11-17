using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Animations;
using Vit.Framework.Mathematics;
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
				}, 2000.Millis(), Easing.InOut )
			}, new() {
				Size = (100, 100),
				Anchor = anchor,
				Origin = anchor
			} );
		}

		bool toggle = true;
		AddChild( new BasicButton {
			Clicked = () => {
#pragma warning disable CS0618 // this is a test. its fine
				if ( toggle ) {
					this.Animate().Mutate( v => v.Padding, (v, s) => v.Padding = s, new Spacing<float>( 100 ), 1000.Millis(), Easing.InOut );
				}
				else {
					this.Animate().Mutate( v => v.Padding, (v, s) => v.Padding = s, new Spacing<float>( 0 ), 1000.Millis(), Easing.InOut );
				}
#pragma warning restore CS0618
				toggle = !toggle;
			}
		}, new() {
			Size = (100, 100),
			Anchor = Anchor.Centre,
			Origin = Anchor.Centre
		} );
	}
}
