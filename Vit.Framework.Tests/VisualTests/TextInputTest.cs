using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Input;

namespace Vit.Framework.Tests.VisualTests;
public class TextInputTest : TestScene {
	public TextInputTest () {
		AddChild( new BasicTextField(), new() {
			Size = (6, 0),
			Anchor = Anchor.Centre,
			Origin = Anchor.Centre
		} );
	}
}
