namespace Vit.Framework.Mathematics.Curves;

public class Spline<T> : ICurve<T>{
	public readonly List<T> Points = new();
	public readonly List<CurveType> Curves = new();

	public static CurveType Type { get; } = CurveType.Custom;

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
}
