using Vit.Framework.Graphics;
using Vit.Framework.Graphics.TwoD;
using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Tests.Layout;

public class GridTest : DrawableLayoutContainer<IDrawableLayoutElement> {
	public GridTest () {
		var grid = new DrawableGridContainer<IDrawableLayoutElement> {
			Rows = new() {
				TrackSizes = new TrackSize<float>[] { 400f, 400f, 400f }
			},
			Columns = new() {
				TrackSizes = new TrackSize<float>[] { 200f, 200f, 200f, 200f, 200f }
			},
			ContentAlignment = Anchor.BottomLeft,
			FlowDirection = FlowDirection.RightThenUp,
			FillDirection = FlowDirection.RightThenUp,
			Gap = new( 20 )
		};
		AddChild( grid, new() {
			Size = new(1f.Relative())
		} );

		for ( int i = 0; i < 3 * 5; i++ ) {
			grid.AddChild( new Sprite { Tint = new ColorHsv<Radians<float>, float>( (i / 5f).Radians(), 1, 1 ).ToRgba() }, new() {
				
			} );
		}
	}
}