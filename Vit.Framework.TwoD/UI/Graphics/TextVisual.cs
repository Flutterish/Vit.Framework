using System.Diagnostics.CodeAnalysis;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Localisation;
using Vit.Framework.Mathematics;
using Vit.Framework.Text.Fonts;
using Vit.Framework.TwoD.Graphics.Text;

namespace Vit.Framework.TwoD.UI.Graphics;

public abstract class TextVisual<T> : Visual<T> where T : DrawableText {
	protected TextVisual ( T displayed ) : base( displayed ) { }

	LocalisableString text;
	LocalisationStore? localisationStore;
	LocalisedString? localised;

	public string RawText {
		get => Displayed.Text;
		set {
			if ( localised != null ) {
				localised.TextChanged -= onLocalisedTextChanged;
				localised = null;
				text = default;
			}

			Displayed.Text = value;
			InvalidateLayout( LayoutInvalidations.Self | LayoutInvalidations.RequiredSize );
		}
	}
	public LocalisableString Text {
		get => text;
		set {
			text = value;
			updateLocalisedString();
		}
	}
	public float FontSize {
		get => Displayed.FontSize;
		set {
			Displayed.FontSize = value;
			InvalidateLayout( LayoutInvalidations.Self | LayoutInvalidations.RequiredSize );
		}
	}

	void updateLocalisedString () {
		if ( localisationStore == null )
			return;

		if ( localised != null ) {
			localised.TextChanged -= onLocalisedTextChanged;
		}
		if ( text.Data == null ) {
			localised = null;
			return;
		}
		localised = localisationStore.GetLocalised( text );
		localised.BindTextChanged( onLocalisedTextChanged );
	}

	void onLocalisedTextChanged ( string value ) {
		Displayed.Text = value;
		InvalidateLayout( LayoutInvalidations.Self | LayoutInvalidations.RequiredSize );
	}

	FontStore fontStore = null!;
	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		fontStore = dependencies.Resolve<FontStore>();
		localisationStore = dependencies.Resolve<LocalisationStore>();
		updateLocalisedString();
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
