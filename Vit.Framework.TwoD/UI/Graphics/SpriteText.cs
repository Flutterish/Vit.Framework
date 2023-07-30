using System.Diagnostics.CodeAnalysis;
using Vit.Framework.Graphics;
using Vit.Framework.TwoD.Graphics.Text;

namespace Vit.Framework.TwoD.UI.Graphics;

public class SpriteText : Visual<DrawableSpriteText> {
	[SetsRequiredMembers]
	public SpriteText ( string text ) {
		Displayed = new() {
			FontSize = 32,
			Text = text,
			Tint = ColorRgba.Black
		};
	}

	protected override void PerformLayout () {
		Displayed.UnitToGlobalMatrix = UnitToGlobalMatrix;
	}
}
