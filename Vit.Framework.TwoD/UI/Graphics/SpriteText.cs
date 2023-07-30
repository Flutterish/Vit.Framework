using System.Diagnostics.CodeAnalysis;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.Text.Fonts;
using Vit.Framework.TwoD.Graphics.Text;

namespace Vit.Framework.TwoD.UI.Graphics;

public class SpriteText : Visual<DrawableSpriteText> {
	[SetsRequiredMembers]
	public SpriteText () {
		Displayed = new();
	}

	public string Text {
		get => Displayed.Text;
		set => Displayed.Text = value;
	}
	public float FontSize {
		get => Displayed.FontSize;
		set => Displayed.FontSize = value;
	}
	public ColorRgba<float> Tint {
		get => Displayed.Tint;
		set => Displayed.Tint = value;
	}

	FontStore fontStore = null!;
	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		fontStore = dependencies.Resolve<FontStore>();
		base.OnLoad( dependencies );
	}

	public Font Font {
		get => Displayed.Font;
		set {
			Displayed.Font = value;
			fontIdentifier = null;
		}
	}
	FontIdentifier? fontIdentifier = FontStore.DefaultFont;
	[MaybeNull]
	public FontIdentifier FontIdentifier {
		get => fontIdentifier;
		set {
			if ( !value.TrySet( ref fontIdentifier ) )
				return;

			Displayed.Font = fontStore.GetFont( value );
		}
	}

	protected override void PerformLayout () {
		Displayed.UnitToGlobalMatrix = UnitToGlobalMatrix;
	}
}
