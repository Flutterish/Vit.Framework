using System.Numerics;

namespace Vit.Framework.Mathematics.Curves;

public struct Line<T, TTime> : ICurve<T, TTime> where T : IInterpolatable<T, TTime> {
	public T Start;
	public T End;

	public static CurveType Type { get; } = CurveType.Line;

	public T Evaluate ( TTime time ) {
		return Start.Lerp( End, time );
	}
}

public struct Line2<T> : ICurve<Point2<T>, T> where T : INumber<T> {
	public Point2<T> Start;
	public Point2<T> End;

	public static CurveType Type { get; } = CurveType.Line;

	public Point2<T> Evaluate ( T time ) {
		return Start.Lerp( End, time );
	}
}

public struct Line3<T> : ICurve<Point3<T>, T> where T : INumber<T> {
	public Point3<T> Start;
	public Point3<T> End;

	public static CurveType Type { get; } = CurveType.Line;

	public Point3<T> Evaluate ( T time ) {
		return Start.Lerp( End, time );
	}
}

public struct Line<T> : ICurve<Point1<T>, T> where T : INumber<T> {
	public Point1<T> Start;
	public Point1<T> End;

	public static CurveType Type { get; } = CurveType.Line;

	public Point1<T> Evaluate ( T time ) {
		return Start.Lerp( End, time );
	}
}
