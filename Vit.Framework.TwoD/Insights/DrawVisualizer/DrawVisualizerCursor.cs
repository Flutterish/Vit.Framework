using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Animations;
using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Animations;
using Vit.Framework.TwoD.UI.Composite;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.TwoD.Insights.DrawVisualizer;

public class DrawVisualizerCursor : InternalContainer {
	BoxCursor cursor = new();
	public DrawVisualizerCursor () {
		AddInternalChild( cursor );
	}

	IViewableInDrawVisualiser? target;
	public IViewableInDrawVisualiser? Target {
		get => target;
		set {
			if ( value.TrySet( ref target ) )
				cursor.Flash();
		}
	}
	protected override void PerformSelfLayout () {
		if ( Target == null ) {
			cursor.Scale = Axes2<float>.Zero;
			return;
		}

		cursor.Scale = Axes2<float>.One;
		var a = ScreenSpaceToLocalSpace( Target.UnitToGlobalMatrix.Apply( new Point2<float>( 0, 0 ) ) );
		var b = ScreenSpaceToLocalSpace( Target.UnitToGlobalMatrix.Apply( new Point2<float>( 1, 1 ) ) );
		var c = ScreenSpaceToLocalSpace( Target.UnitToGlobalMatrix.Apply( new Point2<float>( 1, 0 ) ) );
		var d = ScreenSpaceToLocalSpace( Target.UnitToGlobalMatrix.Apply( new Point2<float>( 0, 1 ) ) );

		AxisAlignedBox2<float> box = new() {
			MinX = float.Min( float.Min( a.X, b.X ), float.Min( c.X, d.X ) ),
			MaxX = float.Max( float.Max( a.X, b.X ), float.Max( c.X, d.X ) ),
			MinY = float.Min( float.Min( a.Y, b.Y ), float.Min( c.Y, d.Y ) ),
			MaxY = float.Max( float.Max( a.Y, b.Y ), float.Max( c.Y, d.Y ) ),
		};

		cursor.Position = box.Position;
		cursor.Size = box.Size;
	}

	public override void Update () {
		base.Update();
		InvalidateLayout( LayoutInvalidations.Self );
	}

	class BoxCursor : LayoutContainer {
		Box background;
		public BoxCursor () {
			AddChild( background = new Box { Tint = ColorRgb.HotPink, Alpha = 0.4f }, new() {
				Size = new( 1f.Relative() )
			} );
			foreach ( var anchor in new[] { Anchor.TopRight, Anchor.TopLeft, Anchor.BottomRight, Anchor.BottomLeft } ) {
				AddChild( new Box { Tint = ColorRgb.HotPink }, new() {
					Size = (20, 14),
					Anchor = anchor,
					Origin = anchor
				} );
				AddChild( new Box { Tint = ColorRgb.HotPink }, new() {
					Size = (14, 20),
					Anchor = anchor,
					Origin = anchor
				} );
			}
		}

		public void Flash () {
			background.Animate()
				.FlashColour( ColorRgb.White, ColorRgb.HotPink, 600.Millis() )
				.FlashAlpha( 0.8f, 0.4f, 200.Millis() );
		}
	}
}
