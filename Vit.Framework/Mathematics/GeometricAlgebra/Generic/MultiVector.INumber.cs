using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace Vit.Framework.Mathematics.GeometricAlgebra.Generic;

public partial record MultiVector<T> : INumber<MultiVector<T>> {
	public int CompareTo ( object? obj ) {
		return obj is MultiVector<T> vec ? CompareTo( vec ) : 0;
	}

	public int CompareTo ( MultiVector<T>? other ) {
		if ( other is null )
			return -1;

		if ( IsScalar && other.IsScalar )
			return Scalar.CompareTo( other.Scalar );

		for ( int i = 0; i < Math.Max( Components.Length, other.Components.Length ); i++ ) {
			if ( i >= Components.Length ) {
				return -1;
			}
			if ( i >= other.Components.Length ) {
				return 1;
			}

			var compBases = Components[i].CompareTo( other.Components[i] );
			if ( compBases != 0 )
				return compBases;
		}

		return 0;
	}

	public bool IsScalar => Components.Length == 0 || (Components.Length == 1 && Components[0].Bases.Length == 0);
	public T Scalar => Components.Length == 0 || Components[0].Bases.Length != 0 ? T.AdditiveIdentity : Components[0].Scale;

	public static bool operator > ( MultiVector<T> left, MultiVector<T> right ) {
		if ( left.IsScalar && right.IsScalar )
			return left.Scalar > right.Scalar;

		return false;
	}

	public static bool operator >= ( MultiVector<T> left, MultiVector<T> right ) {
		if ( left.IsScalar && right.IsScalar )
			return left.Scalar >= right.Scalar;

		return left == right;
	}

	public static bool operator < ( MultiVector<T> left, MultiVector<T> right ) {
		if ( left.IsScalar && right.IsScalar )
			return left.Scalar < right.Scalar;

		return false;
	}

	public static bool operator <= ( MultiVector<T> left, MultiVector<T> right ) {
		if ( left.IsScalar && right.IsScalar )
			return left.Scalar > right.Scalar;

		return left == right;
	}

	public static MultiVector<T> operator % ( MultiVector<T> left, MultiVector<T> right ) {
		return Zero;
	}

	public static MultiVector<T> Abs ( MultiVector<T> value ) {
		return value;
	}

	public static bool IsCanonical ( MultiVector<T> value ) {
		return value.Components.All( x => T.IsCanonical( x.Scale ) );
	}

	public static bool IsComplexNumber ( MultiVector<T> value ) {
		return !value.IsScalar || T.IsComplexNumber( value.Scalar );
	}

	public static bool IsEvenInteger ( MultiVector<T> value ) {
		return value.IsScalar && T.IsEvenInteger( value.Scalar );
	}

	public static bool IsFinite ( MultiVector<T> value ) {
		return value.Components.All( x => T.IsFinite( x.Scale ) );
	}

	public static bool IsImaginaryNumber ( MultiVector<T> value ) {
		return !value.IsScalar || T.IsImaginaryNumber( value.Scalar );
	}

	public static bool IsInfinity ( MultiVector<T> value ) {
		return value.Components.Any( x => T.IsInfinity( x.Scale ) );
	}

	public static bool IsInteger ( MultiVector<T> value ) {
		return value.IsScalar && T.IsInteger( value.Scalar );
	}

	public static bool IsNaN ( MultiVector<T> value ) {
		return value.Components.Any( x => T.IsNaN( x.Scale ) );
	}

	public static bool IsNegative ( MultiVector<T> value ) {
		return value.IsScalar && T.IsNegative( value.Scalar );
	}

	public static bool IsNegativeInfinity ( MultiVector<T> value ) {
		return value.IsScalar && T.IsNegativeInfinity( value.Scalar );
	}

	public static bool IsNormal ( MultiVector<T> value ) {
		return value.Components.All( x => T.IsNormal( x.Scale ) );
	}

	public static bool IsOddInteger ( MultiVector<T> value ) {
		return value.IsScalar && T.IsOddInteger( value.Scalar );
	}

	public static bool IsPositive ( MultiVector<T> value ) {
		return value.IsScalar && T.IsPositive( value.Scalar );
	}

	public static bool IsPositiveInfinity ( MultiVector<T> value ) {
		return value.IsScalar && T.IsPositiveInfinity( value.Scalar );
	}

	public static bool IsRealNumber ( MultiVector<T> value ) {
		return value.IsScalar && T.IsRealNumber( value.Scalar );
	}

	public static bool IsSubnormal ( MultiVector<T> value ) {
		return value.Components.Any( x => T.IsSubnormal( x.Scale ) );
	}

	public static bool IsZero ( MultiVector<T> value ) {
		return value == Zero;
	}

	public static MultiVector<T> MaxMagnitude ( MultiVector<T> x, MultiVector<T> y ) {
		return x.MagnitudeSquared > y.MagnitudeSquared ? x : y;
	}

	public static MultiVector<T> MaxMagnitudeNumber ( MultiVector<T> x, MultiVector<T> y ) {
		return IsNaN(y) || x.MagnitudeSquared > y.MagnitudeSquared ? x : y;
	}

	public static MultiVector<T> MinMagnitude ( MultiVector<T> x, MultiVector<T> y ) {
		return x.MagnitudeSquared < y.MagnitudeSquared ? x : y;
	}

	public static MultiVector<T> MinMagnitudeNumber ( MultiVector<T> x, MultiVector<T> y ) {
		return IsNaN(y) || x.MagnitudeSquared < y.MagnitudeSquared ? x : y;
	}

	public static MultiVector<T> Parse ( ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider ) {
		return new MultiVector<T>( new SimpleBlade<T>[] { new( T.Parse( s, style, provider ), Array.Empty<BasisVector<T>>() ) } );
	}

	public static MultiVector<T> Parse ( string s, NumberStyles style, IFormatProvider? provider ) {
		return new MultiVector<T>( new SimpleBlade<T>[] { new( T.Parse( s, style, provider ), Array.Empty<BasisVector<T>>() ) } );
	}

	public static bool TryParse ( ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen( false )] out MultiVector<T> result ) {
		if ( T.TryParse( s, style, provider, out var scalar ) ) {
			result = new MultiVector<T>( new SimpleBlade<T>[] { new( scalar, Array.Empty<BasisVector<T>>() ) } );
			return true;
		}
		result = null;
		return false;
	}

	public static bool TryParse ( [NotNullWhen( true )] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen( false )] out MultiVector<T> result ) {
		if ( T.TryParse( s, style, provider, out var scalar ) ) {
			result = new MultiVector<T>( new SimpleBlade<T>[] { new( scalar, Array.Empty<BasisVector<T>>() ) } );
			return true;
		}
		result = null;
		return false;
	}

	public static MultiVector<T> One { get; } = new MultiVector<T>( new SimpleBlade<T>[] { new( T.One, Array.Empty<BasisVector<T>>() ) } );
	public static int Radix => T.Radix;
	public static MultiVector<T> Zero { get; } = new MultiVector<T>( new SimpleBlade<T>[] { new( T.Zero, Array.Empty<BasisVector<T>>() ) } );

	public bool TryFormat ( Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider ) {
		throw new NotImplementedException();
	}

	public string ToString ( string? format, IFormatProvider? formatProvider ) {
		throw new NotImplementedException();
	}

	public static MultiVector<T> Parse ( ReadOnlySpan<char> s, IFormatProvider? provider ) {
		throw new NotImplementedException();
	}

	public static bool TryParse ( ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen( false )] out MultiVector<T> result ) {
		throw new NotImplementedException();
	}

	public static MultiVector<T> Parse ( string s, IFormatProvider? provider ) {
		throw new NotImplementedException();
	}

	public static bool TryParse ( [NotNullWhen( true )] string? s, IFormatProvider? provider, [MaybeNullWhen( false )] out MultiVector<T> result ) {
		throw new NotImplementedException();
	}

	public static MultiVector<T> AdditiveIdentity { get; } = new MultiVector<T>( new SimpleBlade<T>[] { new( T.AdditiveIdentity, Array.Empty<BasisVector<T>>() ) } );

	public static MultiVector<T> operator -- ( MultiVector<T> value ) {
		throw new NotImplementedException();
	}

	public static MultiVector<T> operator / ( MultiVector<T> left, MultiVector<T> right ) {
		throw new NotImplementedException();
	}

	public static MultiVector<T> operator ++ ( MultiVector<T> value ) {
		throw new NotImplementedException();
	}

	public static MultiVector<T> MultiplicativeIdentity { get; } = new MultiVector<T>( new SimpleBlade<T>[] { new( T.MultiplicativeIdentity, Array.Empty<BasisVector<T>>() ) } );

	public static MultiVector<T> operator - ( MultiVector<T> value ) {
		return -T.One * value;
	}

	public static MultiVector<T> operator + ( MultiVector<T> value ) {
		return value;
	}

	static bool INumberBase<MultiVector<T>>.TryConvertFromChecked<TOther> ( TOther value, out MultiVector<T> result ) {
		throw new NotImplementedException();
	}

	static bool INumberBase<MultiVector<T>>.TryConvertFromSaturating<TOther> ( TOther value, out MultiVector<T> result ) {
		throw new NotImplementedException();
	}

	static bool INumberBase<MultiVector<T>>.TryConvertFromTruncating<TOther> ( TOther value, out MultiVector<T> result ) {
		throw new NotImplementedException();
	}

	static bool INumberBase<MultiVector<T>>.TryConvertToChecked<TOther> ( MultiVector<T> value, out TOther result ) {
		throw new NotImplementedException();
	}

	static bool INumberBase<MultiVector<T>>.TryConvertToSaturating<TOther> ( MultiVector<T> value, out TOther result ) {
		throw new NotImplementedException();
	}

	static bool INumberBase<MultiVector<T>>.TryConvertToTruncating<TOther> ( MultiVector<T> value, out TOther result ) {
		throw new NotImplementedException();
	}
}
