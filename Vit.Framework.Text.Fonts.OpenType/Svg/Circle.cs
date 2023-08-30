using Vit.Framework.Graphics;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.Curves;

namespace Vit.Framework.Text.Fonts.OpenType.Svg;

public class Circle : SvgElement {
	double cxValue;
	double cyValue;
	double rValue;
	ColorSRgb<byte>? fillValue;
	public override void Open ( ref SvgOutline.Context context ) {
		base.Open( ref context );
		cxValue = 0;
		cyValue = 0;
		rValue = 0;
		fillValue = ColorSRgb.Black.ToByte();
	}

	HeapByteString cx = "cx";
	HeapByteString cy = "cy";
	HeapByteString r = "r";
	HeapByteString fill = "fill";
	public override bool SetAttribute ( ref SvgOutline.Context context, ByteString name, ByteString unescapedValue ) {
		if ( base.SetAttribute( ref context, name, unescapedValue ) )
			return true;

		if ( name == cx ) {
			cxValue = Number.Parse( unescapedValue );
		}
		else if ( name == cy ) {
			cyValue = Number.Parse( unescapedValue );
		}
		else if ( name == r ) {
			rValue = Number.Parse( unescapedValue );
		}
		else if ( name == fill ) {
			fillValue = Color.Parse( unescapedValue );
		}
		//else {
		//	return false;
		//}
		return true;
	}

	public override void Close ( ref SvgOutline.Context context ) {
		base.Close( ref context );
		var a = 1.00005519 * rValue;
		var b = 0.66342686 * rValue;
		var c = 0.99873585 * rValue;
		Vector2<double> offset = (cxValue, cyValue);

		var spline = new Spline2<double>( context.Matrix.Apply(new Point2<double>( 0, a ) + offset) );
		//spline.Color = fillValue;
		//context.Glyph.Outline.Splines.Add( spline );
		void add ( ref SvgOutline.Context context, Point2< double> a, Point2<double> b, Point2<double> c ) {
			spline!.AddCubicBezier(
				context.Matrix.Apply( a + offset ),
				context.Matrix.Apply( b + offset ),
				context.Matrix.Apply( c + offset )
			);
		}

		add( ref context, (b, c), (c, b), (a, 0) );
		add( ref context, (c, -b), (b, -c), (0, -a) );
		add( ref context, (-b, -c), (-c, -b), (-a, 0) );
		add( ref context, (-c, b), (-b, c), (0, a) );
	}
}
