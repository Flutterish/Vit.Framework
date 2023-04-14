namespace Vit.Framework.Mathematics.Curves;

public struct CubicBezierCurve<T> : ICurve<T> {
	public T Start;
	public T ControlPointA;
	public T ControlPointB;
	public T End;

	public static CurveType Type { get; } = CurveType.BezierCubic;
}
