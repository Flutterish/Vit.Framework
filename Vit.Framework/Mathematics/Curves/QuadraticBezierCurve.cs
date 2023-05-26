using System.Numerics;

namespace Vit.Framework.Mathematics.Curves;

public struct QuadraticBezierCurve<T, TTime> : ICurve<T, TTime> where T : IInterpolatable<T, TTime> {
	public T Start;
	public T ControlPoint;
	public T End;

	public static CurveType Type { get; } = CurveType.BezierQuadratic;

	public T Evaluate ( TTime time ) {
		var ab = Start.Lerp( ControlPoint, time );
		var bc = ControlPoint.Lerp( End, time );
		return ab.Lerp( bc, time );
	}
}

public struct QuadraticBezierCurve2<T> : ICurve<Point2<T>, T> where T : INumber<T> {
	public Point2<T> Start;
	public Point2<T> ControlPoint;
	public Point2<T> End;

	public static CurveType Type { get; } = CurveType.BezierQuadratic;

	public Point2<T> Evaluate ( T time ) {
		var ab = Start.Lerp( ControlPoint, time );
		var bc = ControlPoint.Lerp( End, time );
		return ab.Lerp( bc, time );
	}
}

public struct QuadraticBezierCurve3<T> : ICurve<Point3<T>, T> where T : INumber<T> {
	public Point3<T> Start;
	public Point3<T> ControlPoint;
	public Point3<T> End;

	public static CurveType Type { get; } = CurveType.BezierQuadratic;

	public Point3<T> Evaluate ( T time ) {
		var ab = Start.Lerp( ControlPoint, time );
		var bc = ControlPoint.Lerp( End, time );
		return ab.Lerp( bc, time );
	}
}


public struct QuadraticBezierCurve<T> : ICurve<Point1<T>, T> where T : INumber<T> {
	public Point1<T> Start;
	public Point1<T> ControlPoint;
	public Point1<T> End;

	public static CurveType Type { get; } = CurveType.BezierQuadratic;

	public Point1<T> Evaluate ( T time ) {
		var ab = Start.Lerp( ControlPoint, time );
		var bc = ControlPoint.Lerp( End, time );
		return ab.Lerp( bc, time );
	}
}