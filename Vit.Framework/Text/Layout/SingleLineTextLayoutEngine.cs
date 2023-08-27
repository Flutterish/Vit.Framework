using System.Globalization;
using Vit.Framework.Mathematics;
using Vit.Framework.Text.Fonts;

namespace Vit.Framework.Text.Layout;

public static class SingleLineTextLayoutEngine {
	public static int GetBufferLengthFor ( string text )
		=> text.Length + 1;

	/// <summary>
	/// Computes text layout in a single line.
	/// </summary>
	/// <param name="text">The text to compute the layout for.</param>
	/// <param name="font">The font to use when computing the matrics.</param>
	/// <param name="metrics">A buffer that will hold glyph metrics. Must be at least the size given by <see cref="GetBufferLengthFor(string)"/>.</param>
	/// <param name="usedBounds">The bounding box of the generated layout.</param>
	/// <returns>The amount of metrics generated.</returns>
	public static int ComputeLayout ( string text, Font font, Span<GlyphMetric> metrics, out AxisAlignedBox2<double> usedBounds ) {
		var enumerator = StringInfo.GetTextElementEnumerator( text );

		usedBounds = Size2<double>.Zero;
		metrics[0].StartIndex = 0;
		int count = 0;

		Point2<double> current = Point2<double>.Zero;
		while ( enumerator.MoveNext() ) {
			ref var metric = ref metrics[count];

			var glyphVector = metric.GlyphVector = enumerator.GetTextElement().AsMemory();
			metric.EndIndex = metrics[count + 1].StartIndex = enumerator.ElementIndex;

			var glyph = font.GetGlyph( glyphVector );
			metric.Bounds = glyph.DefinedBoundingBox + current.FromOrigin();
			metric.Anchor = current;

			usedBounds = usedBounds.Contain( metric.Bounds );
			current.X += glyph.HorizontalAdvance;
			current.Y += glyph.VerticalAdvance;

			count++;
		}

		return count;
	}
}

public struct GlyphMetric {
	public Point2<double> Anchor;
	public AxisAlignedBox2<double> Bounds;
	public ReadOnlyMemory<char> GlyphVector;
	public int StartIndex;
	public int EndIndex;

	public int Length => EndIndex - StartIndex;
}