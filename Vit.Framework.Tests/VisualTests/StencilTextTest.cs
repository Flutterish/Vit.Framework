using Vit.Framework.Graphics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Graphics;

namespace Vit.Framework.Tests.VisualTests;

public class StencilTextTest : TestScene {
	StencilText text;
	Box sizeBox;

	public StencilTextTest () {
		AddChild( sizeBox = new() { Tint = ColorRgb.Green }, new() {
			Anchor = Anchor.BottomRight,
			Origin = Anchor.BottomRight
		} );

		AddChild( text = new() {
			RawText = "Hello, World!",
			FontSize = 32
		}, new() {
			Size = (0,0), // size is expanded to fit requiredSize
			Anchor = Anchor.BottomRight,
			Origin = Anchor.BottomRight
		} );
	}

	public override void Update () {
		UpdateLayoutParameters( sizeBox, p => p with { Size = text.Size } );
		base.Update();
	}
}
