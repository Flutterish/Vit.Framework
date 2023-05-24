using Vit.Framework.Graphics;
using Vit.Framework.Graphics.TwoD;
using Vit.Framework.Graphics.TwoD.Layout;

namespace Vit.Framework.Tests.VisualTests;

public class SampleTestScene : TestScene {
	public SampleTestScene () {
		AddChild( new Sprite { Tint = ColorRgba.Green }, new() {
			Size = new( 1f.Relative() )
		} );
	}
}
