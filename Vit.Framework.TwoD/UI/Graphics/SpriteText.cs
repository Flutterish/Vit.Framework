using System.Diagnostics.CodeAnalysis;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.Mathematics;
using Vit.Framework.Text.Fonts;
using Vit.Framework.TwoD.Graphics.Text;

namespace Vit.Framework.TwoD.UI.Graphics;

public class SpriteText : Visual<DrawableSpriteText> {
	public SpriteText () : base( new() ) { }

	public string Text {
		get => Displayed.Text;
		set {
			Displayed.Text = value;
			InvalidateLayout( LayoutInvalidations.Self | LayoutInvalidations.RequiredSize );
		}
	}
	public float FontSize {
		get => Displayed.FontSize;
		set {
			Displayed.FontSize = value;
			InvalidateLayout( LayoutInvalidations.Self | LayoutInvalidations.RequiredSize );
		}
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

	public FontCollection Font {
		get => Displayed.Font;
		set {
			Displayed.Font = value;
			fontIdentifier = null;
			InvalidateLayout( LayoutInvalidations.Self | LayoutInvalidations.RequiredSize );
		}
	}
	FontCollectionIdentifier? fontIdentifier = FontStore.DefaultFontCollection;
	[MaybeNull]
	public FontCollectionIdentifier FontIdentifier {
		get => fontIdentifier;
		set {
			if ( !value.TrySet( ref fontIdentifier ) || !IsLoaded )
				return;

			Displayed.Font = fontStore.GetFontCollection( value );
			InvalidateLayout( LayoutInvalidations.Self | LayoutInvalidations.RequiredSize );
		}
	}

	bool useFullGlyphSize = true;
	public bool UseFullGlyphSize {
		get => useFullGlyphSize;
		set {
			if ( value == useFullGlyphSize )
				return;

			useFullGlyphSize = value;
			InvalidateLayout( LayoutInvalidations.Self | LayoutInvalidations.RequiredSize );
		}
	}

	protected override Size2<float> ComputeRequiredSize () {
		return ((useFullGlyphSize ? Displayed.BoundingBox : Displayed.TextBoundingBox).Size * Displayed.MetricMultiplier).Cast<float>();
	}

	protected override void PerformLayout () {
		Displayed.UnitToGlobalMatrix = UnitToGlobalMatrix;
	}
}
