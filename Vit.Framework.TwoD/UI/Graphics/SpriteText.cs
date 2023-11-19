using Vit.Framework.Graphics;
using Vit.Framework.TwoD.Graphics.Text;

namespace Vit.Framework.TwoD.UI.Graphics;

public class SpriteText : TextVisual<DrawableSpriteText> {
	public SpriteText () : base( new() ) { }

	public ColorRgba<float> Tint {
		get => Displayed.Tint;
		set => Displayed.Tint = value;
	}
}
