using Vit.Framework.Graphics;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Layout;

namespace Vit.Framework.Tests.VisualTests;

public class SampleTestSceneA : TestScene {
	public SampleTestSceneA () {
		AddChild( new Sprite { Tint = ColorRgba.Green }, new() {
			Size = new( 1f.Relative() )
		} );
	}
}

public class SampleTestSceneB : TestScene {
	public SampleTestSceneB () {
		AddChild( new Sprite { Tint = ColorRgba.Red }, new() {
			Size = new( 1f.Relative() )
		} );
	}
}
