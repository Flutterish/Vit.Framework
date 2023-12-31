﻿using System.Text;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.Curves;

namespace Vit.Framework.Text.Outlines;

public class SplineOutline : IGlyphOutline {
	public readonly List<Spline2<double>> Splines = new();

	public override string ToString () {
		return ToSvg();
	}

	public string ToSvg () {
		StringBuilder sb = new();
		foreach ( var spline in Splines ) {
			sb.Append( $"M{spline.Points[0].X} {spline.Points[0].Y} " );
			int i = 1;
			void point () {
				sb.Append( $"{spline.Points[i].X:N} {spline.Points[i++].Y:N} " );
			}

			foreach ( var curve in spline.Curves ) {
				if ( curve is CurveType.Line ) {
					sb.Append( 'L' );
					point();
				}
				else if ( curve is CurveType.BezierQuadratic ) {
					sb.Append( 'Q' );
					point();
					point();
				}
				else if ( curve is CurveType.BezierCubic ) {
					sb.Append( 'C' );
					point();
					point();
					point();
				}
				else {
					throw new Exception( "Curve type not convertible to svg" );
				}
			}
			sb.Append( "Z " );
		}

		if ( sb.Length != 0 )
			sb.Length--;

		return sb.ToString();
	}
}

public static class SplineOutlineExtensions {
	public static AxisAlignedBox2<double> CalculateBoundingBox ( this SplineOutline outline ) {
		return outline.Splines.Select( x => x.GetBoundingBox() ).Aggregate( AABox2<double>.Undefined, ( a, b ) => a.Contain( b ) );
	}
}