using System.Numerics;

namespace Vit.Framework.Mathematics.Curves;

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

	public SegmentEnumerator GetEnumerator () => new( this );

	public struct SegmentEnumerator {
		int index;
		int pointIndex;
		Spline<T, TTime> spline;

		public SegmentEnumerator ( Spline<T, TTime> spline ) : this() {
			this.spline = spline;
			index = -1;
		}

		static int getCount ( CurveType curve ) => curve switch {
			CurveType.Line => 1,
			CurveType.BezierQuadratic => 2,
			CurveType.BezierCubic => 3,
			_ => throw new Exception( "Unknown curve type" )
		};

		public bool MoveNext () {
			if ( index != -1 ) {
				pointIndex += getCount( spline.Curves[index] );
			}

			index++;
			return index < spline.Curves.Count;
		}

		public (CurveType type, int index) Current => (spline.Curves[index], pointIndex);
	}
}

public class Spline2<T> : Spline<Point2<T>, T> where T : INumber<T> {
	public Spline2 ( Point2<T> startPoint ) : base( startPoint ) { }
}

public static class SplineExtensions {
	public static AxisAlignedBox2<T> GetBoundingBox<T> ( this Spline<Point2<T>, T> spline ) where T : INumber<T>, IFloatingPointIeee754<T> {
		var box = AABox<T>.Undefined;

		foreach ( var (type, start) in spline ) {
			T minx;
			T maxx;
			T miny;
			T maxy;

			if ( type == CurveType.Line ) {
				var a = spline.Points[start];
				var b = spline.Points[start + 1];

				minx = CurveTypeExtensions.LineMin( a.X, b.X );
				maxx = CurveTypeExtensions.LineMax( a.X, b.X );
				miny = CurveTypeExtensions.LineMin( a.Y, b.Y );
				maxy = CurveTypeExtensions.LineMax( a.Y, b.Y );
			}
			else if ( type == CurveType.BezierQuadratic ) {
				var a = spline.Points[start];
				var b = spline.Points[start + 1];
				var c = spline.Points[start + 2];

				minx = CurveTypeExtensions.QuadraticBezierMin( a.X, b.X, c.X );
				maxx = CurveTypeExtensions.QuadraticBezierMax( a.X, b.X, c.X );
				miny = CurveTypeExtensions.QuadraticBezierMin( a.Y, b.Y, c.Y );
				maxy = CurveTypeExtensions.QuadraticBezierMax( a.Y, b.Y, c.Y );
			}
			else if ( type == CurveType.BezierCubic ) {
				var a = spline.Points[start];
				var b = spline.Points[start + 1];
				var c = spline.Points[start + 2];
				var d = spline.Points[start + 3];

				minx = CurveTypeExtensions.CubicBezierMin( a.X, b.X, c.X, d.X );
				maxx = CurveTypeExtensions.CubicBezierMax( a.X, b.X, c.X, d.X );
				miny = CurveTypeExtensions.CubicBezierMin( a.Y, b.Y, c.Y, d.Y );
				maxy = CurveTypeExtensions.CubicBezierMax( a.Y, b.Y, c.Y, d.Y );
			}
			else {
				throw new Exception( "Unknown curve type" );
			}

			box = box.Contain( new AxisAlignedBox2<T> {
				MinX = minx,
				MaxX = maxx,
				MinY = miny,
				MaxY = maxy
			} );
		}

		return box;
	}
}

public class Spline3<T> : Spline<Point3<T>, T> where T : INumber<T> {
	public Spline3 ( Point3<T> startPoint ) : base( startPoint ) { }
}

public class Spline<T> : Spline<Scalar<T>, T> where T : INumber<T> {
	public Spline ( Scalar<T> startPoint ) : base( startPoint ) { }
}