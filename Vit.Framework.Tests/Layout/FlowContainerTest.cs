﻿using Vit.Framework.Graphics;
using Vit.Framework.Graphics.TwoD;
using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Tests.Layout;

public class FlowContainerTest : LayoutContainer<ILayoutElement> {
	const float margin = 20;
	const float padding = 50;

	public FlowContainerTest () {
		AddChild( new Sprite { Tint = ColorRgba.Blue }, new() {
			Size = new( 1f.Relative() )
		} );
		AddChild( new LayoutContainer<ILayoutElement> {
			Padding = new( padding ),
			LayoutChildren = new (ILayoutElement, LayoutParams)[] {
				(new Sprite { Tint = ColorRgba.Green }, new() { Size = new( 1f.Relative() ) })
			}
		}, new() {
			Size = new( 1f.Relative() )
		} );

		AddChild( createFlowContainer(), new() {
			Size = new( 1f.Relative() )
		} );
	}

	FlowContainer<ILayoutElement> createFlowContainer () {
		FlowContainer<ILayoutElement> container = new() {
			Padding = new( padding ),
			FlowOrigin = Anchor.TopRight,
			FlowDirection = FlowDirection.DownThenLeft,
			//CollapseMargins = false
		};

		for ( int i = 0; i < 30; i++ ) {
			container.AddChild( new LayoutContainer<Sprite> {
				Padding = new( -margin ),
				LayoutChildren = new (Sprite, LayoutParams)[] {
					(new Sprite { Tint = ColorRgba.White }, new() { Size = new( 1f.Relative() ) }),
					(new Sprite { Tint = new ColorHsv<Radians<float>, float>( (i / 5f).Radians(), 1, 1 ).ToRgba() }, new() {
						Origin = Anchor.Centre,
						Anchor = Anchor.Centre,
						Size = new( (i + 1) * 10 )
					} )
				}
			}, new() {
				Margins = new( margin ),
				Size = new() { Size = new( (i + 1) * 10 ) }
			} );
		}

		return container;
	}
}