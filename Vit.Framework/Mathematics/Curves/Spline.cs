using System.Numerics;

namespace Vit.Framework.Mathematics.Curves;

public class Spline<T> : Spline<Scalar<T>, T> where T : INumber<T> {
	public Spline ( Scalar<T> startPoint ) : base( startPoint ) { }
}

public class Spline2<T> : Spline<Point2<T>, T> where T : INumber<T> {
	public Spline2 ( Point2<T> startPoint ) : base( startPoint ) { }
}

public class Spline3<T> : Spline<Point3<T>, T> where T : INumber<T> {
	public Spline3 ( Point3<T> startPoint ) : base( startPoint ) { }
}

public class Spline<T, TTime> where T : IInterpolatable<T, TTime> where TTime : INumber<TTime> {
	public readonly List<T> Points = new();
	public readonly List<CurveType> Curves = new();

	public Spline ( T startPoint ) {
		Points.Add( startPoint );
	}

	public void AddLine ( T to ) {
		Curves.Add( CurveType.Line );
		Points.Add( to );
	}

	public void AddQuadraticBezier ( T cp, T to ) {
		Curves.Add( CurveType.BezierQuadratic );
		Points.Add( cp );
		Points.Add( to );
	}

	public void AddCubicBezier ( T cp1, T cp2, T to ) {
		Curves.Add( CurveType.BezierCubic );
		Points.Add( cp1 );
		Points.Add( cp2 );
		Points.Add( to );
	}

	public IEnumerable<T> GetNonControlPoints () {
		int i = 0;
		yield return Points[i++];
		foreach ( var curve in Curves ) {
			if ( curve == CurveType.Line ) {
				yield return Points[i++];
			}
			else if ( curve == CurveType.BezierQuadratic ) {
				i += 1;
				yield return Points[i++];
			}
			else if ( curve == CurveType.BezierCubic ) {
				i += 2;
				yield return Points[i++];
			}
			else {
				throw new Exception( "Unknown curve type" );
			}
		}
	}

	public IEnumerable<T> GetPoints ( int samples = 32 ) {
		var deltaTime = TTime.One / TTime.CreateChecked( samples );

		int i = 0;
		T last = Points[i++];
		yield return last;
		foreach ( var curve in Curves ) {
			if ( curve == CurveType.Line ) {
				last = Points[i++];
				yield return last;
			}
			else if ( curve == CurveType.BezierQuadratic ) {
				var bezier = new QuadraticBezierCurve<T, TTime> {
					Start = last,
					ControlPoint = Points[i++],
					End = Points[i++]
				};
				last = bezier.End;

				var time = TTime.Zero;
				for ( int j = 0; j < samples; j++ ) {
					yield return bezier.Evaluate( time );

					time += deltaTime;
				}
				yield return last;
			}
			else if ( curve == CurveType.BezierCubic ) {
				var bezier = new CubicBezierCurve<T, TTime> {
					Start = last,
					ControlPointA = Points[i++],
					ControlPointB = Points[i++],
					End = Points[i++]
				};
				last = bezier.End;

				var time = TTime.Zero;
				for ( int j = 0; j < samples; j++ ) {
					yield return bezier.Evaluate( time );

					time += deltaTime;
				}
				yield return last;
			}
			else {
				throw new Exception( "Unknown curve type" );
			}
		}
	}
}