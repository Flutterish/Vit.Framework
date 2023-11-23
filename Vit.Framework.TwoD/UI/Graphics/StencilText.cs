using Vit.Framework.Graphics;
using Vit.Framework.TwoD.Graphics.Text;
using Vit.Framework.TwoD.UI.Animations;

namespace Vit.Framework.TwoD.UI.Graphics;

public class StencilText : TextVisual<DrawableStencilText>, IHasPremultipliedTint {
	public StencilText () : base( new() ) { }

	public ColorRgb<float> Tint {
		get => Displayed.Tint;
		set => Displayed.Tint = value;
	}
	public float Alpha {
		get => Displayed.Alpha;
		set => Displayed.Alpha = value;
	}

}
