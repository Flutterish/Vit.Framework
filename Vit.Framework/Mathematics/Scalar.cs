using System.Numerics;

namespace Vit.Framework.Mathematics;

public struct Scalar<T> : IInterpolatable<Scalar<T>, T> where T : INumber<T> {
	public T Value;

	public Scalar ( T value ) {
		Value = value;
	}

	public static implicit operator T ( Scalar<T> scalar )
		=> scalar.Value;

	public static implicit operator Scalar<T> ( T value )
		=> new( value );

	public Scalar<T> Lerp ( Scalar<T> goal, T time ) {
		return Value.Lerp( goal.Value, time );
	}
}
