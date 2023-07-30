using Vit.Framework.Graphics;
using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.Tests.Layout;

public class FlexboxTest : LayoutContainer<UIComponent> {
	public FlexboxTest () {
		var flexbox = new Flexbox<UIComponent> {
			ContentAlignment = Anchor.TopLeft,
			FlowDirection = FlowDirection.RightThenDown,
			Padding = new( 50 ),
			Gap = new( 10 )
		};
		AddChild( flexbox, new() {
			Size = new(1f.Relative())
		} );

		for ( int i = 0; i < 8; i++ ) {
			flexbox.AddChild( new Box { Tint = new ColorHsv<Radians<float>, float>( (i / 5f).Radians(), 1, 1 ).ToRgba() }, new() {
				Size = new() {
					Base = new Size2<float>( 500, 250 ),
					MaxWidth = 600,
					MinWidth = 300
				},
				Grow = 1
			} );
		}
	}
}