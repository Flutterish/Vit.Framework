using System.Collections.Immutable;
using System.Numerics;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics.LinearAlgebra.Generic;

public record Vector<T> where T : INumber<T> {
	public readonly ImmutableArray<T> Components;

	public Vector ( params T[] components ) : this( components.AsSpan() ) { }

	public Vector ( ReadOnlySpan<T> components ) {
		var length = components.Length;
		while ( length != 0 && components[length - 1] == T.AdditiveIdentity )
			length--;

		Components = components[..length].ToImmutableArray();
	}

	public T this[int index] => index < Components.Length ? Components[index] : T.AdditiveIdentity;

	public T Dot ( Vector<T> other ) {
		var sum = T.AdditiveIdentity;
		var length = System.Math.Max( Components.Length, other.Components.Length );
		for ( int i = 0; i < length; i++ )
			sum += this[i] * other[i];

		return sum;
	}

	public Vector<T> Cross ( Vector<T> other ) {
		using var components = new RentedArray<T>( 3 );
		foreach ( ref var i in components )
			i = T.AdditiveIdentity;

		for ( int i = 0; i < 3; i++ ) {
			for ( int j = 0; j < 3; j++ ) {
				if ( i == j )
					continue;

				int dim = (i != 0 && j != 0) ? 0 : (i != 1 && j != 1) ? 1 : 2;
				var value = this[i] * other[j];
				if ( i < j )
					components[dim] += value;
				else
					components[dim] -= value;
			}
		}

		return new( components );
	}

	public static Vector<T> operator * ( Vector<T> vector, T scalar )
		=> scalar * vector;
	public static Vector<T> operator * ( T scalar, Vector<T> vector ) {
		using var array = new RentedArray<T>( vector.Components.Length );
		foreach ( ref var i in array )
			i *= scalar;


		return new( array );
	}

	public static Vector<T> operator + ( Vector<T> left, Vector<T> right ) {
		using var array = new RentedArray<T>( System.Math.Max( left.Components.Length, right.Components.Length ) );
		for ( int i = 0; i < array.Length; i++ ) {
			array[i] = left[i] + right[i];
		}
		return new( array );
	}
	public static Vector<T> operator - ( Vector<T> left, Vector<T> right ) {
		using var array = new RentedArray<T>( System.Math.Max( left.Components.Length, right.Components.Length ) );
		for ( int i = 0; i < array.Length; i++ ) {
			array[i] = left[i] - right[i];
		}
		return new( array );
	}

	public static Vector<T> operator + ( Vector<T> vector )
		=> vector;
	public static Vector<T> operator - ( Vector<T> vector )
		=> -T.MultiplicativeIdentity * vector;

	public static implicit operator Matrix<T> ( Vector<T> vector ) {
		return new Matrix<T>( new ReadOnlySpan2D<T>( vector.Components.AsSpan(), 1, vector.Components.Length ) );
	}

	public override string ToString () {
		return $"[{string.Join("; ", Components)}]";
	}
}
