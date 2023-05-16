using System.Numerics;

namespace Vit.Framework.Mathematics.GeometricAlgebra;

public struct MultiVector4<T> where T : INumber<T> {
	public T Scalar;
	public Vector4<T> Vector;
	public BiVector4<T> BiVector;
	public TriVector4<T> TriVector;
	public FourVector4<T> QuadVector;
}
