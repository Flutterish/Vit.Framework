using System.Globalization;
using Vit.Framework.Mathematics;
using Vit.Framework.Text.Fonts;

namespace Vit.Framework.Text.Layout;

public static class SingleLineTextLayoutEngine {
	public static int GetBufferLengthFor ( string text )
		=> text.Length;

	/// <summary>
	/// Computes text layout in a single line.
	/// </summary>
	/// <param name="text">The text to compute the layout for.</param>
	/// <param name="font">The font to use when computing the matrics.</param>
	/// <param name="metrics">A buffer that will hold glyph metrics. Must be at least the size given by <see cref="GetBufferLengthFor(string)"/>.</param>
	/// <param name="usedBounds">The bounding box of the generated layout.</param>
	/// <returns>The amount of metrics generated.</returns>
	public static int ComputeLayout ( string text, FontCollection font, Span<GlyphMetric> metrics, out AxisAlignedBox2<double> usedBounds ) {
		var enumerator = StringInfo.GetTextElementEnumerator( text );

		usedBounds = Size2<double>.Zero;
		int count = 0;

		Point2<double> current = Point2<double>.Zero;
		while ( enumerator.MoveNext() ) {
			ref var metric = ref metrics[count];

			var glyphVector = metric.GlyphVector = enumerator.GetTextElement().AsMemory();
			metric.StartIndex = enumerator.ElementIndex;
			metric.EndIndex = enumerator.ElementIndex + glyphVector.Length;

			var glyph = font.GetGlyph( glyphVector.Span );
			metric.Glyph = glyph;
			metric.Anchor = current;
			metric.SizeMultiplier = font.PrimaryUnitsPerEm / glyph.Font.UnitsPerEm;

			usedBounds = usedBounds.Contain( new AxisAlignedBox2<double>() { 
				MinX = metric.Glyph.DefinedBoundingBox.MinX * metric.SizeMultiplier,
				MinY = metric.Glyph.DefinedBoundingBox.MinY * metric.SizeMultiplier,
				MaxX = metric.Glyph.DefinedBoundingBox.MaxX * metric.SizeMultiplier,
				MaxY = metric.Glyph.DefinedBoundingBox.MaxY * metric.SizeMultiplier
			} + current.FromOrigin() );
			current.X += glyph.HorizontalAdvance * metric.SizeMultiplier;
			current.Y += glyph.VerticalAdvance * metric.SizeMultiplier;
			usedBounds = usedBounds.Contain( new Size2<double>( current.X, current.Y ) );

			count++;
		}

		return count;
	}
}

public struct GlyphMetric {
	public Point2<double> Anchor;
	public Glyph Glyph;
	public ReadOnlyMemory<char> GlyphVector;
	public int StartIndex;
	public int EndIndex;

	public double SizeMultiplier;

	public int Length => EndIndex - StartIndex;

	public static bool IsWhiteSpace ( GlyphMetric metric ) {
		if ( metric.GlyphVector.Length != 1 )
			return false;

		return string.IsNullOrWhiteSpace( new string( metric.GlyphVector.Span ) );
	}
}