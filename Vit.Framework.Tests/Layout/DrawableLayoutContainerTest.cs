using Vit.Framework.Graphics;
using Vit.Framework.Graphics.TwoD;
using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Graphics.TwoD.Layout;

namespace Vit.Framework.Tests.Layout;

public class DrawableLayoutContainerTest : DrawableLayoutContainer<IDrawableLayoutElement> {
	public DrawableLayoutContainerTest () {
		Padding = new( 100 );

		AddChild( new Sprite { Tint = ColorRgba.Green }, new() {
			Size = new(1f.Relative(), 1f.Relative())
		} );

		AddChild( new Sprite { Tint = ColorRgba.HotPink }, new() {
			Size = new(100, 100),
			Origin = Anchor.BottomLeft,
			Anchor = Anchor.BottomLeft
		} );
		AddChild( new Sprite { Tint = ColorRgba.HotPink }, new() {
			Size = new( 100, 100 ),
			Origin = Anchor.TopRight,
			Anchor = Anchor.TopRight
		} );
		AddChild( new Sprite { Tint = ColorRgba.HotPink }, new() {
			Size = new( 10, 1f.Relative() ),
			Origin = Anchor.Centre,
			Anchor = Anchor.Centre
		} );
		AddChild( new Sprite { Tint = ColorRgba.HotPink }, new() {
			Size = new( 1f.Relative(), 10 ),
			Origin = Anchor.Centre,
			Anchor = Anchor.Centre
		} );
		AddChild( new Sprite { Tint = ColorRgba.HotPink }, new() {
			Size = new( 100, 100 ),
			Origin = Anchor.BottomRight,
			Anchor = Anchor.TopLeft
		} );
	}
}
