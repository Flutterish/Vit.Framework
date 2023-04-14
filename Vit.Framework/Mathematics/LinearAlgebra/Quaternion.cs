using System.Numerics;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Quaternion<T> where T : INumber<T>, IFloatingPointIeee754<T> {
	public T I;
	public T J;
	public T K;
	public T W;

	public static Quaternion<T> FromAxisAngle<TAngle> ( Vector3<T> axis, TAngle angle ) where TAngle : IAngle<TAngle, T> {
		var two = T.One + T.One;
		var halfAngle = angle / two;
		var s = TAngle.Sin( halfAngle );
		var c = TAngle.Cos( halfAngle );

		return new() {
			I = axis.X * s,
			J = axis.Y * s,
			K = axis.Z * s,
			W = c
		};
	}
}
