using Vit.Framework.DependencyInjection;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;
using Vit.Framework.Text.Fonts;
using Vit.Framework.Text.Layout;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.Graphics.Text;

public abstract class DrawableText : Drawable {
	FontCollection font = null!;
	public FontCollection Font {
		get => font;
		set {
			if ( value.TrySet( ref font ) )
				invalidateTextLayout();
		}
	}

	float fontSize = 16;
	public float FontSize {
		get => fontSize;
		set {
			if ( value.TrySet( ref fontSize ) )
				InvalidateDrawNodes();
		}
	}

	string text = string.Empty;
	public string Text {
		get => text;
		set {
			if ( value.TrySet( ref text ) )
				invalidateTextLayout();
		}
	}

	public DrawableText () {
		layout = new RentedArray<GlyphMetric>( 8 ) with { Length = 0 };
	}

	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		base.OnLoad( dependencies );
		Font ??= dependencies.Resolve<FontStore>().GetFontCollection( FontStore.DefaultFontCollection );
	}

	SharedResourceInvalidations textLayoutInvalidations;
	void invalidateTextLayout () {
		InvalidateDrawNodes();
		textLayoutInvalidations.Invalidate();
		isLayoutComputed = false;
	}

	bool isLayoutComputed = false;
	RentedArray<GlyphMetric> layout;
	public ReadOnlySpan<GlyphMetric> TextLayout {
		get {
			if ( !isLayoutComputed )
				recomputeLayout();

			return layout;
		}
	}

	AxisAlignedBox2<double> boundingBox;
	public AxisAlignedBox2<double> TextBoundingBox {
		get {
			if ( !isLayoutComputed )
				recomputeLayout();

			return boundingBox;
		}
	}

	public AxisAlignedBox2<double> BoundingBox {
		get {
			if ( !isLayoutComputed )
				recomputeLayout();

			return boundingBox with { MinY = 0, MaxY = Font.PrimaryUnitsPerEm };
		}
	}

	public AxisAlignedBox2<double> ComputeBoundingBoxByGlyphIndex ( int startIndex, int length ) {
		if ( startIndex >= TextLayout.Length ) {
			if ( TextLayout.Length == 0 ) {
				return new AxisAlignedBox2<double> {
					MinY = 0,
					MaxY = Font.PrimaryUnitsPerEm
				};
			}

			var _right = TextLayout[^1];

			return new AxisAlignedBox2<double> {
				MinY = 0,
				MaxY = Font.PrimaryUnitsPerEm,
				MinX = _right.Glyph.HorizontalAdvance * _right.SizeMultiplier + _right.Anchor.X,
				MaxX = _right.Glyph.HorizontalAdvance * _right.SizeMultiplier + _right.Anchor.X
			};
		}

		var left = TextLayout[startIndex];

		if ( length == 0 ) {
			return new AxisAlignedBox2<double> {
				MinX = left.Anchor.X,
				MaxX = left.Anchor.X,
				MinY = 0,
				MaxY = Font.PrimaryUnitsPerEm
			};
		}

		var right = TextLayout[startIndex + length - 1];

		return new AxisAlignedBox2<double> {
			MinY = 0,
			MaxY = Font.PrimaryUnitsPerEm,
			MinX = left.Anchor.X,
			MaxX = right.Glyph.HorizontalAdvance * right.SizeMultiplier + right.Anchor.X
		};
	}

	public double MetricMultiplier => FontSize / Font.PrimaryUnitsPerEm;

	void recomputeLayout () {
		var minSize = SingleLineTextLayoutEngine.GetBufferLengthFor( Text );
		layout.ReallocateStorage( minSize );
		layout.Length = minSize;

		layout.Length = SingleLineTextLayoutEngine.ComputeLayout( Text, Font, layout, out boundingBox );
	}

	public override void Dispose () {
		layout.Dispose();
		base.Dispose();
	}

	public abstract class TextDrawNode<TSource> : DrawableDrawNode<TSource> where TSource : DrawableText {
		public TextDrawNode ( TSource source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		SharedResourceUpload textLayoutUpload;
		protected double MetricMultiplier;
		protected FontCollection Font = null!;
		protected string Text = null!;
		protected override void UpdateState () {
			base.UpdateState();
			textLayoutUpload = Source.textLayoutInvalidations.GetUpload();
			Text = Source.Text;
			Font = Source.Font;
			MetricMultiplier = Source.MetricMultiplier;
		}

		protected bool ValidateLayout () {
			return textLayoutUpload.Validate( ref Source.textLayoutInvalidations );
		}
	}
}
