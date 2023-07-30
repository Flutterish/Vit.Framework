using Vit.Framework.Graphics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.Tests.UI;
public class LayoutContainerTest : LayoutContainer {
	public LayoutContainerTest () {
		Padding = new( 100 );
		AutoSizeDirection = LayoutDirection.Both;

		AddChild( new Box { Tint = ColorRgba.Green }, new() {
			Size = new( 1f.Relative(), 1f.Relative() )
		} );

		AddChild( new Box { Tint = ColorRgba.HotPink }, new() {
			Size = new( 100, 100 ),
			Origin = Anchor.BottomLeft,
			Anchor = Anchor.BottomLeft
		} );
		AddChild( new Box { Tint = ColorRgba.HotPink }, new() {
			Size = new( 100, 100 ),
			Origin = Anchor.TopRight,
			Anchor = Anchor.TopRight
		} );
		AddChild( new Box { Tint = ColorRgba.HotPink }, new() {
			Size = new( 10, 1f.Relative() ),
			Origin = Anchor.Centre,
			Anchor = Anchor.Centre
		} );
		AddChild( new Box { Tint = ColorRgba.HotPink }, new() {
			Size = new( 1f.Relative(), 10 ),
			Origin = Anchor.Centre,
			Anchor = Anchor.Centre
		} );
		AddChild( new Box { Tint = ColorRgba.HotPink }, new() {
			Size = new( 100, 100 ),
			Origin = Anchor.BottomRight,
			Anchor = Anchor.TopLeft
		} );
	}
}
