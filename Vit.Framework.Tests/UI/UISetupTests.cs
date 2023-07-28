using Vit.Framework.Graphics;
using Vit.Framework.Graphics.TwoD;
using Vit.Framework.Graphics.TwoD.UI;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Tests.UI;

public class UISetupTests : CompositeDrawable<IDrawable> {
	Visual root;
	public UISetupTests () {
		AddInternalChild( new DrawableUI( root = new Sprite { Tint = ColorRgba.HotPink } ) );

		root.Size = new( 100 );
		root.Position = new( 10 );
	}

	DateTime startTime = DateTime.Now;
	public override void Update () {
		var t = ((float)(DateTime.Now - startTime).TotalSeconds).Radians();
		var s = (Radians<float>.Cos( t ) + 1) / 2;
		root.Size = new( 100 + s * 1 );

		base.Update();
	}
}
