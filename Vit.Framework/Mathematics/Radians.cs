using System.Numerics;

namespace Vit.Framework.Mathematics;

public struct Radians<T> : IAngle<Radians<T>, T> where T : INumber<T>, ITrigonometricFunctions<T> {
	public T Value;

	public Radians ( T value ) {
		Value = value;
	}

	static readonly T degreeFactor = T.CreateChecked( 180 ) / T.Pi;
	public Degrees<T> AsDegrees () => new Degrees<T>( Value * degreeFactor );
	public static implicit operator Degrees<T> ( Radians<T> radians )
		=> radians.AsDegrees();

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

	public static T operator / ( Radians<T> left, Radians<T> right ) {
		return left.Value / right.Value;
	}

	public static Radians<T> Zero { get; } = T.Zero.Radians();
	public static Radians<T> FullRotation { get; } = T.Tau.Radians();

	public static Radians<T> operator % ( Radians<T> left, Radians<T> right ) {
		return new(left.Value % right.Value);
	}

	public static Radians<T> operator - ( Radians<T> value ) {
		return new(-value.Value);
	}

	public static bool operator > ( Radians<T> left, Radians<T> right ) {
		return left.Value > right.Value;
	}

	public static bool operator >= ( Radians<T> left, Radians<T> right ) {
		return left > right || left == right;
	}

	public static bool operator < ( Radians<T> left, Radians<T> right ) {
		return left.Value < right.Value;
	}

	public static bool operator <= ( Radians<T> left, Radians<T> right ) {
		return left < right || left == right;
	}

	public static bool operator == ( Radians<T> left, Radians<T> right ) {
		return left.Value == right.Value;
	}

	public static bool operator != ( Radians<T> left, Radians<T> right ) {
		return left.Value != right.Value;
	}

	public int CompareTo ( Radians<T> other ) {
		return this > other ? 1 : this < other ? -1 : 0;
	}

	public bool Equals ( Radians<T> other ) {
		return this == other;
	}

	public override int GetHashCode () {
		return HashCode.Combine( Value );
	}

	public override bool Equals ( object? obj ) {
		return obj is Radians<T> other && this == other;
	}
}

public static class RadiansExtensions {
	public static Radians<T> Radians<T> ( this T value ) where T : INumber<T>, ITrigonometricFunctions<T> {
		return new Radians<T>( value );
	}
}
