using System.Numerics;

namespace Vit.Framework.Mathematics.GeometricAlgebra;

public struct TriVector4<T> where T : INumber<T> {
	public T XYZ;
	public T XYW;
	public T XZW;
	public T YZW;
}
