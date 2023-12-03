using System.Diagnostics.CodeAnalysis;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Localisation;
using Vit.Framework.Mathematics;
using Vit.Framework.Text.Fonts;
using Vit.Framework.TwoD.Graphics.Text;

namespace Vit.Framework.TwoD.UI.Graphics;

public abstract class TextVisual<T> : Visual<T> where T : DrawableText {
	protected TextVisual ( T displayed ) : base( displayed ) {
		localised.ValueChanged += text => {
			Displayed.Text = text;
			InvalidateLayout( LayoutInvalidations.Self | LayoutInvalidations.RequiredSize );
		};
	}

	LocalisationStore.LocalisedString localised = new();

	public string RawText {
		get => Displayed.Text;
		set => localised.SetRaw( value );
	}
	public LocalisableString? Text {
		get => localised.LocalisableString;
		set => localised.LocalisableString = value;
	}
	public float FontSize {
		get => Displayed.FontSize;
		set {
			Displayed.FontSize = value;
			InvalidateLayout( LayoutInvalidations.Self | LayoutInvalidations.RequiredSize );
		}
	}

	FontStore fontStore = null!;
	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		fontStore = dependencies.Resolve<FontStore>();
		localised.Store = dependencies.Resolve<LocalisationStore>();

		base.OnLoad( dependencies );
	}

	protected override void OnUnload () {
		localised.Store = null;

		base.OnUnload();
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
