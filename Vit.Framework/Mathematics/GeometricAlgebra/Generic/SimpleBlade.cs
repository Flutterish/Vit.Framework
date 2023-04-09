using System.Buffers;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Vit.Framework.Mathematics.GeometricAlgebra.Generic;

public record SimpleBlade<T> : IComparable<SimpleBlade<T>> where T : INumber<T> {
	public T Scale { get; init; }
	public ImmutableArray<BasisVector<T>> Bases { get; init; }

	public bool AreBasesEqual ( SimpleBlade<T> other ) {
		if ( Bases.Length != other.Bases.Length )
			return false;

		for ( int i = 0; i < Bases.Length; i++ )
			if ( Bases[i] != other.Bases[i] )
				return false;

		return true;
	}

	public int CompareTo ( SimpleBlade<T>? other ) {
		if ( other is null )
			return -1;

		if ( Bases.Length != other.Bases.Length )
			return Bases.Length - other.Bases.Length;

		for ( int i = 0; i < Bases.Length; i++ )
			if ( Bases[i] != other.Bases[i] )
				return Bases[i].SortingOrder - other.Bases[i].SortingOrder;

		return Scale.CompareTo( other.Scale );
	}

	public SimpleBlade ( T scale, ReadOnlySpan<BasisVector<T>> bases ) : this( scale, rentBases( bases ), bases.Length ) { }

	static BasisVector<T>[] rentBases ( ReadOnlySpan<BasisVector<T>> bases ) {
		var rentedBases = ArrayPool<BasisVector<T>>.Shared.Rent( bases.Length );
		bases.CopyTo( rentedBases );
		return rentedBases;
	}

	SimpleBlade ( T scale, BasisVector<T>[] bases, int count ) {
		bool isSorted = false;
		int end = count - 1;
		while ( !isSorted ) {
			isSorted = true;
			for ( int i = 0; i < end; i++ ) {
				if ( bases[i].SortingOrder > bases[i + 1].SortingOrder ) {
					isSorted = false;
					(bases[i], bases[i + 1]) = (bases[i + 1], bases[i]);
					if ( bases[i].SwapFlip && bases[i + 1].SwapFlip )
						scale = -scale;
				}
			}

			end--;
		}

		for ( int i = count - 1; i > 0; i-- ) {
			if ( bases[i] != bases[i - 1] || !bases[i].CanSquare )
				continue;

			scale *= bases[i].Square;
			for ( int j = i + 1; j < count; j++ ) {
				bases[j - 2] = bases[j];
			}
			count -= 2;
		}

		Scale = scale;
		Bases = bases.AsSpan( 0, count ).ToImmutableArray(); 
		ArrayPool<BasisVector<T>>.Shared.Return( bases );
	}

	public static SimpleBlade<T> operator * ( SimpleBlade<T> left, SimpleBlade<T> right ) {
		var count = left.Bases.Length + right.Bases.Length;
		var bases = ArrayPool<BasisVector<T>>.Shared.Rent( count );
		left.Bases.CopyTo( bases, 0 );
		right.Bases.CopyTo( bases, left.Bases.Length );
		return new SimpleBlade<T>( left.Scale * right.Scale, bases, count );
	}

	public static implicit operator MultiVector<T> ( SimpleBlade<T> value ) {
		return new MultiVector<T>( MemoryMarshal.CreateReadOnlySpan( ref value, 1 ) );
	}

	public static MultiVector<T> operator + ( SimpleBlade<T> left, SimpleBlade<T> right ) {
		return (MultiVector<T>)left + (MultiVector<T>)right;
	}
	public static MultiVector<T> operator + ( MultiVector<T> left, SimpleBlade<T> right ) {
		return left + (MultiVector<T>)right;
	}
	public static MultiVector<T> operator + ( SimpleBlade<T> left, MultiVector<T> right ) {
		return (MultiVector<T>)left + right;
	}
	public static MultiVector<T> operator + ( BasisVector<T> left, SimpleBlade<T> right ) {
		return (MultiVector<T>)left + (MultiVector<T>)right;
	}
	public static MultiVector<T> operator + ( SimpleBlade<T> left, BasisVector<T> right ) {
		return (MultiVector<T>)left + (MultiVector<T>)right;
	}

	public static MultiVector<T> operator - ( SimpleBlade<T> left, SimpleBlade<T> right ) {
		return (MultiVector<T>)left - (MultiVector<T>)right;
	}
	public static MultiVector<T> operator - ( MultiVector<T> left, SimpleBlade<T> right ) {
		return left - (MultiVector<T>)right;
	}
	public static MultiVector<T> operator - ( SimpleBlade<T> left, MultiVector<T> right ) {
		return (MultiVector<T>)left - right;
	}
	public static MultiVector<T> operator - ( BasisVector<T> left, SimpleBlade<T> right ) {
		return (MultiVector<T>)left - (MultiVector<T>)right;
	}
	public static MultiVector<T> operator - ( SimpleBlade<T> left, BasisVector<T> right ) {
		return (MultiVector<T>)left - (MultiVector<T>)right;
	}

	public static MultiVector<T> operator * ( MultiVector<T> left, SimpleBlade<T> right ) {
		return left * (MultiVector<T>)right;
	}
	public static MultiVector<T> operator * ( SimpleBlade<T> left, MultiVector<T> right ) {
		return (MultiVector<T>)left * right;
	}

	public string ToString ( bool showSign ) {
		var scale = showSign ? Scale : T.Abs( Scale );
		if ( Bases.Length == 0 )
			return $"{scale}";

		string bases = string.Join( "", Bases.Select( x => x.Name ) );
		if ( scale == T.MultiplicativeIdentity )
			return bases;
		else if ( scale == -T.MultiplicativeIdentity )
			return $"-{bases}";
		else if ( scale == T.AdditiveIdentity )
			return $"{scale}";
		else
			return $"{scale}{bases}";
	}

	public override string ToString () {
		return ToString( showSign: true );
	}
}
