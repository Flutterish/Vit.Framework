using System.Buffers;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Vit.Framework.Math.GeometricAlgebra.Generic;

public record MultiVector<T> where T : INumber<T> {
	public ImmutableArray<SimpleBlade<T>> Components { get; init; }
	public MultiVector ( ReadOnlySpan<SimpleBlade<T>> components ) {
		var count = 0;
		var rented = ArrayPool<SimpleBlade<T>>.Shared.Rent( components.Length );
		
		foreach ( var i in components ) {
			bool isNew = true;
			foreach ( ref var j in rented.AsSpan( 0, count ) ) {
				if ( i.AreBasesEqual( j ) ) {
					j = j with { Scale = i.Scale + j.Scale };
					isNew = false;
					break;
				}
			}

			if ( isNew ) {
				rented[count++] = i;
			}
		}

		for ( int i = count - 1; i >= 0; i-- ) {
			if ( rented[i].Scale != T.Zero )
				continue;

			for ( int j = i; j < count - 1; j++ ) {
				rented[j] = rented[j + 1];
			}
			count--;
		}

		Array.Sort( rented, 0, count );
		Components = rented.AsSpan( 0, count ).ToImmutableArray();
		ArrayPool<SimpleBlade<T>>.Shared.Return( rented );
	}

	public static MultiVector<T> operator * ( MultiVector<T> left, MultiVector<T> right ) {
		int count = left.Components.Length * right.Components.Length;
		var components = ArrayPool<SimpleBlade<T>>.Shared.Rent( count );
		int n = 0;
		foreach ( var i in left.Components ) {
			foreach ( var j in right.Components ) {
				components[n++] = i * j;
			}
		}

		var result = new MultiVector<T>( components.AsSpan( 0, count ) );
		ArrayPool<SimpleBlade<T>>.Shared.Return( components );
		return result;
	}

	public T InnerProduct ( MultiVector<T> other ) {
		other = this * other;
		if ( other.Components.Length == 0 )
			return T.Zero;
		if ( other.Components[0].Bases.Length != 0 )
			return T.Zero;
		return other.Components[0].Scale;
	}

	public MultiVector<T> OuterProduct ( MultiVector<T> other ) {
		return this * other - InnerProduct( other );
	}

	public T MagnitudeSquared => InnerProduct( this );

	public static MultiVector<T> operator + ( MultiVector<T> left, MultiVector<T> right ) {
		var count = left.Components.Length + right.Components.Length;
		var components = ArrayPool<SimpleBlade<T>>.Shared.Rent( count );
		left.Components.CopyTo( components, 0 );
		right.Components.CopyTo( components, left.Components.Length );
		var result = new MultiVector<T>( components.AsSpan( 0, count ) );
		ArrayPool<SimpleBlade<T>>.Shared.Return( components );
		return result;
	}

	public static MultiVector<T> operator - ( MultiVector<T> left, MultiVector<T> right ) {
		var count = left.Components.Length + right.Components.Length;
		var components = ArrayPool<SimpleBlade<T>>.Shared.Rent( count );
		left.Components.CopyTo( components, 0 );
		right.Components.CopyTo( components, left.Components.Length );
		for ( int i = left.Components.Length; i < count; i++ ) {
			components[i] = components[i] with { Scale = -components[i].Scale };
		}
		var result = new MultiVector<T>( components.AsSpan( 0, count ) );
		ArrayPool<SimpleBlade<T>>.Shared.Return( components );
		return result;
	}

	public static implicit operator MultiVector<T> ( T scalar ) {
		var blade = new SimpleBlade<T>( scalar, ReadOnlySpan<BasisVector<T>>.Empty );
		return new MultiVector<T>( MemoryMarshal.CreateReadOnlySpan( ref blade, 1 ) );
	}

	public override string ToString () {
		if ( Components.Length == 0 )
			return "0";

		StringBuilder sb = new();

		bool isFirst = true;
		foreach ( var i in Components ) {
			if ( isFirst ) {
				sb.Append( i.ToString() );
				isFirst = false;
			}
			else {
				sb.Append( T.IsNegative( i.Scale ) ? " - " : " + " );
				sb.Append( i.ToString( showSign: false ) );
			}
		}

		return sb.ToString();
	}
}