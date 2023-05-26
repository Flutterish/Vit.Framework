using System.Numerics;

namespace Vit.Framework.Mathematics.Curves;

public struct CubicBezierCurve<T, TTime> : ICurve<T, TTime> where T : IInterpolatable<T, TTime> {
	public T Start;
	public T ControlPointA;
	public T ControlPointB;
	public T End;

	public static CurveType Type { get; } = CurveType.BezierCubic;

	public T Evaluate ( TTime time ) {
		var ab = Start.Lerp( ControlPointA, time );
		var bc = ControlPointA.Lerp( ControlPointB, time );
		var cd = ControlPointB.Lerp( End, time );

		var c1 = ab.Lerp( bc, time );
		var c2 = bc.Lerp( cd, time );

		return c1.Lerp( c2, time );
	}
}

public struct CubicBezierCurve2<T> : ICurve<Point2<T>, T> where T : INumber<T> {
	public Point2<T> Start;
	public Point2<T> ControlPointA;
	public Point2<T> ControlPointB;
	public Point2<T> End;

	public static CurveType Type { get; } = CurveType.BezierCubic;

	public Point2<T> Evaluate ( T time ) {
		var ab = Start.Lerp( ControlPointA, time );
		var bc = ControlPointA.Lerp( ControlPointB, time );
		var cd = ControlPointB.Lerp( End, time );

		var c1 = ab.Lerp( bc, time );
		var c2 = bc.Lerp( cd, time );

		return c1.Lerp( c2, time );
	}
}

public struct CubicBezierCurve3<T> : ICurve<Point3<T>, T> where T : INumber<T> {
	public Point3<T> Start;
	public Point3<T> ControlPointA;
	public Point3<T> ControlPointB;
	public Point3<T> End;

	public static CurveType Type { get; } = CurveType.BezierCubic;

	public Point3<T> Evaluate ( T time ) {
		var ab = Start.Lerp( ControlPointA, time );
		var bc = ControlPointA.Lerp( ControlPointB, time );
		var cd = ControlPointB.Lerp( End, time );

		var c1 = ab.Lerp( bc, time );
		var c2 = bc.Lerp( cd, time );

		return c1.Lerp( c2, time );
	}
}

public struct CubicBezierCurve<T> : ICurve<Point1<T>, T> where T : INumber<T> {
	public Point1<T> Start;
	public Point1<T> ControlPointA;
	public Point1<T> ControlPointB;
	public Point1<T> End;

	public static CurveType Type { get; } = CurveType.BezierCubic;

	public Point1<T> Evaluate ( T time ) {
		var ab = Start.Lerp( ControlPointA, time );
		var bc = ControlPointA.Lerp( ControlPointB, time );
		var cd = ControlPointB.Lerp( End, time );

		var c1 = ab.Lerp( bc, time );
		var c2 = bc.Lerp( cd, time );

		return c1.Lerp( c2, time );
	}
}
