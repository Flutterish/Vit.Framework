using Vit.Framework.Graphics;
using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.Tests.UI;

public class FlowContainerTest : LayoutContainer<UIComponent> {
	const float margin = 20;
	const float padding = 50;

	public FlowContainerTest () {
		AddChild( new Box { Tint = ColorRgba.Blue }, new() {
			Size = new( 1f.Relative() )
		} );
		AddChild( new LayoutContainer {
			Padding = new( padding ),
			LayoutChildren = new (UIComponent, LayoutParams)[] {
				(new Box { Tint = ColorRgba.Green }, new() { Size = new( 1f.Relative() ) })
			}
		}, new() {
			Size = new( 1f.Relative() )
		} );

		AddChild( createFlowContainerWithRelativeSizes(), new() {
			Size = new( 1f.Relative() )
		} );
	}

	FlowContainer<UIComponent> createFlowContainer () {
		FlowContainer<UIComponent> container = new() {
			Padding = new( padding ),
			ContentAlignment = Anchor.TopRight,
			FlowDirection = FlowDirection.DownThenLeft,
			//CollapseMargins = false
		};

		for ( int i = 0; i < 30; i++ ) {
			container.AddChild( new LayoutContainer<Visual<Sprite>> {
				Padding = new( -margin ),
				LayoutChildren = new (Visual<Sprite>, LayoutParams)[] {
					(new Box { Tint = ColorRgba.White }, new() { Size = new( 1f.Relative() ) }),
					(new Box { Tint = new ColorHsv<Radians<float>, float>( (i / 5f).Radians(), 1, 1 ).ToRgba() }, new() {
						Origin = Anchor.Centre,
						Anchor = Anchor.Centre,
						Size = new( (i + 1) * 10 )
					} )
				}
			}, new() {
				Margins = new( margin ),
				Size = new Size2<float>( (i + 1) * 10 )
			} );
		}

		return container;
	}

	FlowContainer<UIComponent> createFlowContainerWithRelativeSizes () {
		FlowContainer<UIComponent> container = new() {
			Padding = new( padding ),
			ContentAlignment = Anchor.TopCentre,
			FlowDirection = FlowDirection.RightThenDown,
			//CollapseMargins = false,
			ItemJustification = Justification.SpaceBetween,
			ItemAlignment = Alignment.Center,
			LineJustification = LineJustification.SpaceBetween
		};

		for ( int i = 0; i < 30; i++ ) {
			container.AddChild( new Box {
				Tint = new ColorHsv<Radians<float>, float>( (i / 5f).Radians(), 1, 1 ).ToRgba()
			}, new() {
				Margins = new( margin ),
				Size = new() {
					Base = new() {
						Width = (i % 2) == 0 ? (i + 1) * 10 : 0.2f.Relative(),
						Height = 1f.Relative()
					},
					MinHeight = (i + 1) * 10
				}
			} );
		}

		return container;
	}
}
