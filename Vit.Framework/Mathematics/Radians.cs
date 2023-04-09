using System.Numerics;

namespace Vit.Framework.Mathematics;

public struct Radians<T> : IAngle<Radians<T>, T> where T : ITrigonometricFunctions<T> {
	public T Value;

	public Radians ( T value ) {
		Value = value;
	}

	static readonly T degreeFactor = T.CreateChecked( 180 ) / T.Pi;
	public Degrees<T> AsDegrees () => new Degrees<T>( Value * degreeFactor );

	public static T Cos ( Radians<T> value )=> T.Cos( value.Value );
	public static T Sin ( Radians<T> value ) => T.Sin( value.Value );
	public static T Tan ( Radians<T> value ) => T.Tan( value.Value );
	public static T Acos ( Radians<T> value ) => T.Acos( value.Value );
	public static T Asin ( Radians<T> value ) => T.Asin( value.Value );
	public static T Atan ( Radians<T> value ) => T.Atan( value.Value );

	public override string ToString () {
		return $"{Value} radians";
	}

	public static Radians<T> operator * ( Radians<T> left, T right ) {
		return new( left.Value * right );
	}
	public static Radians<T> operator * ( T left, Radians<T> right ) {
		return new( left * right.Value );
	}
	public static Radians<T> operator / ( Radians<T> left, T right ) {
		return new( left.Value / right );
	}
	public static Radians<T> operator + ( Radians<T> left, Radians<T> right ) {
		return new( left.Value + right.Value );
	}
	public static Radians<T> operator - ( Radians<T> left, Radians<T> right ) {
		return new( left.Value - right.Value );
	}
}

public static class RadiansExtensions {
	public static Radians<T> Radians<T> ( this T value ) where T : ITrigonometricFunctions<T> {
		return new Radians<T>( value );
	}
}
