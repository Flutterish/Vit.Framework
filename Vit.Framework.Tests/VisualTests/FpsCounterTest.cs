using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Components;

namespace Vit.Framework.Tests.VisualTests;

public class FpsCounterTest : TestScene {
	public FpsCounterTest () {
		AddChild( new FpsCounter(), new() {
			Size = new( 1f.Relative() )
		} );
	}
}
