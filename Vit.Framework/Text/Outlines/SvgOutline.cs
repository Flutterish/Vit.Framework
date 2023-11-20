using Vit.Framework.Graphics;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.Curves;

namespace Vit.Framework.Text.Outlines;

public class SvgOutline : IGlyphOutline {
	public List<SvgElement> Elements = new();
}

public struct SvgElement { // TODO clip-rule, opacity
	public required Spline2<double>[] Splines;
	public ColorRgba<byte>? Fill;
	public FillRule FillRule;
}

public enum FillRule {
	EvenOdd,
	NonZero
}

public static class SvgOutlineExtensions {
	public static AxisAlignedBox2<double> CalculateBoundingBox ( this SvgOutline outline ) {
		return outline.Elements.SelectMany( x => x.Splines ).Select( x => x.GetBoundingBox() ).Aggregate( AABox2<double>.Undefined, ( a, b ) => a.Contain( b ) );
	}
}