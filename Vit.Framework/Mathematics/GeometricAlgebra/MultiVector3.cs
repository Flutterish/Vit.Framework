using System.Numerics;

namespace Vit.Framework.Mathematics.GeometricAlgebra;

public struct MultiVector3<T> where T : INumber<T> {
	public T Scalar;
	public Vector3<T> Vector;
	public BiVector3<T> BiVector;
	public TriVector3<T> TriVector;
}
