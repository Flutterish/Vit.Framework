using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.Curves;

namespace Vit.Framework.Text.Outlines;

public class SvgOutline : IGlyphOutline {
	public List<SvgSpline> Splines = new();
}

public class SvgSpline : Spline2<double> {
	public ColorSRgba<byte>? Fill;

	public SvgSpline ( Point2<double> startPoint ) : base( startPoint ) { }
}
