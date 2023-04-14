namespace Vit.Framework.Mathematics.Curves;

public struct Line<T> : ICurve<T> {
	public T Start;
	public T End;

	public static CurveType Type { get; } = CurveType.Line;
}
