using Vit.Framework.Graphics;
using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.TwoD.UI.Input;

public class BasicTabVisualizer : CompositeUIComponent {
	BoxCursor cursor = new();
	public BasicTabVisualizer () {
		AddInternalChild( cursor );
	}

	public UIComponent? Target;
	protected override void PerformSelfLayout () {
		if ( Target == null ) {
			cursor.Scale = Axes2<float>.Zero;
			return;
		}

		cursor.Scale = Axes2<float>.One;
		var a = Target.LocalSpaceToAnotherSpace( (0, 0), this );
		var b = Target.LocalSpaceToAnotherSpace( (Target.Width, Target.Height), this );
		var c = Target.LocalSpaceToAnotherSpace( (Target.Width, 0), this );
		var d = Target.LocalSpaceToAnotherSpace( (0, Target.Height), this );

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
		public BoxCursor () {
			foreach ( var anchor in new[] { Anchor.TopRight, Anchor.TopLeft, Anchor.BottomRight, Anchor.BottomLeft } ) {
				AddChild( new Box { Tint = ColorRgba.HotPink }, new() {
					Size = (20, 14),
					Anchor = anchor,
					Origin = anchor
				} );
				AddChild( new Box { Tint = ColorRgba.HotPink }, new() {
					Size = (14, 20),
					Anchor = anchor,
					Origin = anchor
				} );
			}
		}
	}
}
