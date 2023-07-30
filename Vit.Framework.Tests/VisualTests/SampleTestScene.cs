using Vit.Framework.Graphics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Graphics;

namespace Vit.Framework.Tests.VisualTests;

public class SampleTestSceneA : TestScene {
	public SampleTestSceneA () {
		AddChild( new Box { Tint = ColorRgba.Green }, new() {
			Size = new( 1f.Relative() )
		} );
	}
}

public class SampleTestSceneB : TestScene {
	public SampleTestSceneB () {
		AddChild( new Box { Tint = ColorRgba.Red }, new() {
			Size = new( 1f.Relative() )
		} );
	}
}
