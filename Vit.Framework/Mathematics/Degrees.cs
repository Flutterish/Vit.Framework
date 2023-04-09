using System.Numerics;

namespace Vit.Framework.Mathematics;

public struct Degrees<T> : IAngle<Degrees<T>, T> where T : ITrigonometricFunctions<T> {
	public T Value;

	public Degrees ( T value ) {
		Value = value;
	}

	static readonly T radianFactor = T.Pi / T.CreateChecked( 180 );
	public Radians<T> AsRadians () => new Radians<T>( Value * radianFactor );

	public static T Cos ( Degrees<T> value ) => T.Cos( value.Value * radianFactor );
	public static T Sin ( Degrees<T> value ) => T.Sin( value.Value * radianFactor );
	public static T Tan ( Degrees<T> value ) => T.Tan( value.Value * radianFactor );
	public static T Acos ( Degrees<T> value ) => T.Acos( value.Value * radianFactor );
	public static T Asin ( Degrees<T> value ) => T.Asin( value.Value * radianFactor );
	public static T Atan ( Degrees<T> value ) => T.Atan( value.Value * radianFactor );

	public override string ToString () {
		return $"{Value} degrees";
	}

	public static Degrees<T> operator * ( Degrees<T> left, T right ) {
		return new( left.Value * right );
	}
	public static Degrees<T> operator * ( T left, Degrees<T> right ) {
		return new( left * right.Value );
	}
	public static Degrees<T> operator / ( Degrees<T> left, T right ) {
		return new( left.Value / right );
	}
	public static Degrees<T> operator + ( Degrees<T> left, Degrees<T> right ) {
		return new( left.Value + right.Value );
	}
	public static Degrees<T> operator - ( Degrees<T> left, Degrees<T> right ) {
		return new( left.Value - right.Value );
	}
}

public static class DegreesExtensions {
	public static Degrees<T> Degrees<T> ( this T value ) where T : ITrigonometricFunctions<T> {
		return new Degrees<T>( value );
	}
}
