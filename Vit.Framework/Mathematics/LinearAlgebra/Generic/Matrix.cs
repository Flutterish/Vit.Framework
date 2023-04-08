using System.Collections.Immutable;
using System.Numerics;
using System.Text;
using Vit.Framework.Mathematics.GeometricAlgebra.Generic;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics.LinearAlgebra.Generic;

public record Matrix<T> where T : INumber<T> {
	readonly ImmutableArray<T> components;
	public ReadOnlySpan2D<T> Components => new( components.AsSpan(), Rows, Columns );
	public readonly int Rows;
	public readonly int Columns;

	public ReadOnlySpan<T> GetRow ( int y ) => components.AsSpan( y * Columns, Columns );
	public ReadOnlySpanView<T> GetColumn ( int x ) => new( components.AsSpan( x.. ), stride: Columns );

	public T this[int x, int y] => x >= Columns || y >= Rows ? T.AdditiveIdentity : components[y * Columns + x];

	public Matrix ( ReadOnlySpan2D<T> values ) {
		Rows = values.Height;
		Columns = values.Width;

		bool isEmpty = true;
		while ( Rows != 0 ) {
			var lastRow = values.GetRow( Rows - 1 );
			foreach ( var i in lastRow ) {
				if ( i != T.AdditiveIdentity ) {
					isEmpty = false;
					break;
				}
			}

			if ( isEmpty )
				Rows--;
			else
				break;
		}

		isEmpty = true;
		while ( Columns != 0 ) {
			var lastColumn = values.GetColumn( Columns - 1 );
			foreach ( var i in lastColumn ) {
				if ( i != T.AdditiveIdentity ) {
					isEmpty = false;
					break;
				}
			}

			if ( isEmpty )
				Columns--;
			else
				break;
		}

		if ( Columns == 0 || Rows == 0 )
			Columns = Rows = 0;

		using var components = new RentedArray<T>( Columns * Rows );
		for ( int x = 0; x < Columns; x++ ) {
			for ( int y = 0; y < Rows; y++ ) {
				components[y * Columns + x] = values[x, y];
			}
		}

		this.components = components.AsSpan().ToImmutableArray();
	}

	public static Matrix<T> operator * ( Matrix<T> left, Matrix<T> right ) {
		var rows = left.Rows;
		var columns = right.Columns;
		var length = Math.Max( left.Columns, right.Rows );
		using var data = new RentedArray<T>( rows * columns );
		for ( int x = 0; x < columns; x++ ) {
			for ( int y = 0; y < rows; y++ ) {
				var dot = T.AdditiveIdentity;
				for ( int i = 0; i < length; i++ ) {
					dot += left[i, y] * right[x, i];
				}

				data[y * columns + x] = dot;
			}
		}

		return new Matrix<T>( new ReadOnlySpan2D<T>( data, columns, rows ) );
	}

	public override string ToString () {
		StringBuilder sb = new();
		sb.AppendLine( "<" );
		for ( int i = 0; i < Rows; i++ ) {
			var row = GetRow( i );
			sb.Append( "\t[" );
			for ( int j = 0; j < row.Length; j++ ) {
				sb.Append( row[j] );
				if ( j != row.Length - 1 )
					sb.Append( "; " );
			}

			if ( i != Rows - 1 )
				sb.AppendLine( "]," );
			else
				sb.AppendLine( "]" );
		}
		sb.Append( ">" );

		return sb.ToString();
	}

	public static string GenerateMultiplication ( int columns, int rows ) {
		Matrix<MultiVector<T>> generateMatrix ( string name ) {
			var data = new MultiVector<T>[columns, rows];
			for ( int x = 0; x < columns; x++ ) {
				for ( int y = 0; y < rows; y++ ) {
					data[x, y] = new BasisVector<T>( $"{name}[{y * columns + x}]" );
				}
			}

			return new( data );
		}

		return ( generateMatrix( "this" ) * generateMatrix( "other" ) ).ToString();
	}
}
