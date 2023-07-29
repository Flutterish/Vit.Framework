using Vit.Framework.Graphics;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.UI;

namespace Vit.Framework.Tests.UI;

public class UISetupTests : CompositeDrawable<Drawable> {
	public UISetupTests () {
		AddInternalChild( new UITest() );
	}

	public override void Update () {
		base.Update();
	}

	class UITest : CompositeUIComponent<UIComponent> {
		public UITest () {
			Children = new UIComponent[] {
				new Visual { 
					Displayed = new Sprite { Tint = ColorRgba.HotPink },
					Position = new( 10 ),
					Size = new( 100 )
				},
				new Visual {
					Displayed = new Sprite { Tint = ColorRgba.Violet },
					Position = new( 110 ),
					Size = new( 60 )
				}
			};
		}
	}
}
