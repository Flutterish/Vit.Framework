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

	public AxisAlignedBox2<double> ComputeBoundingBoxByGlyphIndex ( int startIndex, int length ) {
		Vector2<double>? start = null;
		Vector2<double>? end = null;

		var advance = Vector2<double>.Zero;
		int index = 0;
		foreach ( var metric in TextLayout ) {
			if ( index == startIndex ) {
				start = advance;
			}
			if ( index - startIndex == length ) {
				end = advance;
				goto end;
			}

			var glyph = Font.GetGlyph( metric.GlyphVector );
			advance.X += glyph.HorizontalAdvance;

			index++;
		}

		end:
		if ( start == null ) {
			start = advance;
		}
		if ( end == null ) {
			end = advance;
		}

		return new AxisAlignedBox2<double> {
			MinY = 0,
			MaxY = Font.UnitsPerEm,
			MinX = double.Min( start.Value.X, end.Value.X ),
			MaxX = double.Max( start.Value.X, end.Value.X )
		};
	}

	public double MetricMultiplier => FontSize / Font.UnitsPerEm;

	void recomputeLayout () {
		var minSize = SingleLineTextLayoutEngine.GetBufferLengthFor( Text );
		layout.ReallocateStorage( minSize );

		SingleLineTextLayoutEngine.ComputeLayout( Text, Font, layout, out boundingBox );
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
			Font = Source.Font;
			Text = Source.Text;
			FontSize = Source.FontSize / Font.UnitsPerEm;
		}

		protected bool ValidateLayout () {
			return textLayoutUpload.Validate( ref Source.textLayoutInvalidations );
		}
	}
}
