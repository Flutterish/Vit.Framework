namespace Vit.Framework.Mathematics.Curves;

public struct QuadraticBezierCurve<T> : ICurve<T> {
	public T Start;
	public T ControlPoint;
	public T End;

	public static CurveType Type { get; } = CurveType.BezierQuadratic;
}
