using System.Numerics;

namespace Vit.Framework.Mathematics.GeometricAlgebra;

public struct MultiVector2<T> where T : INumber<T> {
	public T Scalar;
	public Vector2<T> Vector;
	public BiVector2<T> BiVector;
}
