using Vit.Framework.Mathematics;
using Vit.Framework.Memory;
using Vit.Framework.Text.Fonts;
using Vit.Framework.Text.Layout;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.Graphics.Text;

public abstract class DrawableText : Drawable {
	Font font = null!;
	public Font Font {
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
		layout = new RentedArray<GlyphMetric>( 64 ) with { Length = 0 };
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

			return boundingBox with { MinY = 0, MaxY = Font.UnitsPerEm };
		}
	}

	public AxisAlignedBox2<double> ComputeBoundingBoxByGlyphIndex ( int startIndex, int length ) {
		if ( startIndex >= TextLayout.Length ) {
			if ( TextLayout.Length == 0 ) {
				return new AxisAlignedBox2<double> {
					MinY = 0,
					MaxY = Font.UnitsPerEm
				};
			}

			var _right = TextLayout[^1];

			return new AxisAlignedBox2<double> {
				MinY = 0,
				MaxY = Font.UnitsPerEm,
				MinX = _right.Glyph.DefinedBoundingBox.MaxX + _right.Anchor.X,
				MaxX = _right.Glyph.DefinedBoundingBox.MaxX + _right.Anchor.X
			};
		}

		var left = TextLayout[startIndex];

		if ( length == 0 ) {
			return new AxisAlignedBox2<double> {
				MinX = left.Glyph.DefinedBoundingBox.MinX + left.Anchor.X,
				MaxX = left.Glyph.DefinedBoundingBox.MinX + left.Anchor.X,
				MinY = 0,
				MaxY = Font.UnitsPerEm
			};
		}

		var right = TextLayout[startIndex + length - 1];

		return new AxisAlignedBox2<double> {
			MinY = 0,
			MaxY = Font.UnitsPerEm,
			MinX = left.Glyph.DefinedBoundingBox.MinX + left.Anchor.X,
			MaxX = right.Glyph.DefinedBoundingBox.MaxX + right.Anchor.X
		};
	}

	public double MetricMultiplier => FontSize / Font.UnitsPerEm;

	void recomputeLayout () {
		var minSize = SingleLineTextLayoutEngine.GetBufferLengthFor( Text );
		layout.ReallocateStorage( minSize );

		layout.Length = SingleLineTextLayoutEngine.ComputeLayout( Text, Font, layout, out boundingBox );
	}

	protected override void Dispose ( bool disposing ) {
		layout.Dispose();
		base.Dispose( disposing );
	}

	public abstract class TextDrawNode<TSource> : DrawableDrawNode<TSource> where TSource : DrawableText {
		public TextDrawNode ( TSource source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		SharedResourceUpload textLayoutUpload;
		protected double FontSize;
		protected Font Font = null!;
		protected string Text = null!;
		protected override void UpdateState () {
			base.UpdateState();
			textLayoutUpload = Source.textLayoutInvalidations.GetUpload();
			Text = Source.Text;
			Font = Source.Font;
			FontSize = Source.MetricMultiplier;
		}

		protected bool ValidateLayout () {
			return textLayoutUpload.Validate( ref Source.textLayoutInvalidations );
		}
	}
}
