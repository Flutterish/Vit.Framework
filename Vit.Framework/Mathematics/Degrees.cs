using System.Numerics;

namespace Vit.Framework.Mathematics;

public struct Degrees<T> : IAngle<Degrees<T>, T> where T : INumber<T>, ITrigonometricFunctions<T> {
	public T Value;

	public Degrees ( T value ) {
		Value = value;
	}

	static readonly T radianFactor = T.Pi / T.CreateChecked( 180 );
	public Radians<T> AsRadians () => new Radians<T>( Value * radianFactor );
	public static implicit operator Radians<T> ( Degrees<T> degrees )
		=> degrees.AsRadians();

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

	public static T operator / ( Degrees<T> left, Degrees<T> right ) {
		return left.Value / right.Value;
	}

	public static Degrees<T> Zero { get; } = T.Zero.Degrees();
	public static Degrees<T> FullRotation { get; } = T.CreateChecked( 360 ).Degrees();

	public static Degrees<T> operator % ( Degrees<T> left, Degrees<T> right ) {
		return new(left.Value % right.Value);
	}

	public static Degrees<T> operator - ( Degrees<T> value ) {
		return new( -value.Value );
	}

	public static bool operator > ( Degrees<T> left, Degrees<T> right ) {
		return left.Value > right.Value;
	}

	public static bool operator >= ( Degrees<T> left, Degrees<T> right ) {
		return left > right || left == right;
	}

	public static bool operator < ( Degrees<T> left, Degrees<T> right ) {
		return left.Value < right.Value;
	}

	public static bool operator <= ( Degrees<T> left, Degrees<T> right ) {
		return left < right || left == right;
	}

	public static bool operator == ( Degrees<T> left, Degrees<T> right ) {
		return left.Value == right.Value;
	}

	public static bool operator != ( Degrees<T> left, Degrees<T> right ) {
		return left.Value != right.Value;
	}

	public int CompareTo ( Degrees<T> other ) {
		return this > other ? 1 : this < other ? -1 : 0;
	}

	public bool Equals ( Degrees<T> other ) {
		return this == other;
	}

	public override bool Equals ( object? obj ) {
		return obj is Degrees<T> other && this == other;
	}

	public override int GetHashCode () {
		return HashCode.Combine( Value );
	}
}

public static class DegreesExtensions {
	public static Degrees<T> Degrees<T> ( this T value ) where T : INumber<T>, ITrigonometricFunctions<T> {
		return new Degrees<T>( value );
	}
}
