using System.Numerics;

namespace Vit.Framework.Mathematics.GeometricAlgebra;

public struct MultiVector1<T> where T : INumber<T> {
	public T Scalar;
	public Vector1<T> Vector;
}
