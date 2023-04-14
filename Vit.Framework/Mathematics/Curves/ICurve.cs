namespace Vit.Framework.Mathematics.Curves;

public interface ICurve<T, TTime> {
	abstract static CurveType Type { get; }

	T Evaluate ( TTime time );
}
