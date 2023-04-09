using System.Collections.Immutable;
using System.Numerics;
using System.Text;
using Vit.Framework.Mathematics.GeometricAlgebra.Generic;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics.LinearAlgebra.Generic;

public record Matrix<T> where T : INumber<T> {
	readonly ImmutableArray<T> components;
	public ReadOnlySpan2D<T> Components => new( components.AsSpan(), Columns, Rows );
	public readonly int Rows;
	public readonly int Columns;

	public ReadOnlySpan<T> GetRow ( int y ) => components.AsSpan( y * Columns, Columns );
	public ReadOnlySpanView<T> GetColumn ( int x ) => new( components.AsSpan( x.. ), stride: Columns );

	public T this[int x, int y] => (x >= Columns || y >= Rows) ? T.AdditiveIdentity : components[y * Columns + x];

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

	public Matrix<T> Transposed () {
		using var data = new RentedArray<T>( components.Length );
		for ( int x = 0; x < Columns; x++ ) {
			for ( int y = 0; y < Rows; y++ ) {
				data[x * Rows + y] = components[y * Columns + x];
			}
		}

		return new Matrix<T>( new Span2D<T>( data, Rows, Columns ) );
	}

	public static Matrix<T> operator * ( Matrix<T> left, Matrix<T> right ) {
		var rows = left.Rows;
		var columns = right.Columns;
		var length = Math.Min( left.Columns, right.Rows );
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
		return ToString( Components );
	}

	public static string ToString ( ReadOnlySpan2D<T> data ) {
		StringBuilder sb = new();
		sb.AppendLine( "<" );
		for ( int i = 0; i < data.Height; i++ ) {
			var row = data.GetRow( i );
			sb.Append( "\t[" );
			for ( int j = 0; j < row.Length; j++ ) {
				sb.Append( row[j] );
				if ( j != row.Length - 1 )
					sb.Append( "; " );
			}

			if ( i != data.Height - 1 )
				sb.AppendLine( "]," );
			else
				sb.AppendLine( "]" );
		}
		sb.Append( ">" );

		return sb.ToString();
	}

	public static Matrix<MultiVector<T>> GenerateLabelMatrix ( string name, int columns, int rows ) {
		var data = new MultiVector<T>[rows, columns];
		for ( int x = 0; x < columns; x++ ) {
			for ( int y = 0; y < rows; y++ ) {
				data[y, x] = new BasisVector<T>( $"{name}[{x},{y}]" );
			}
		}

		return new( data );
	}

	public static Matrix<MultiVector<T>> GenerateLabelMatrix ( string name, int columns, int rows, string componentNames ) {
		var data = new MultiVector<T>[rows, columns];
		for ( int x = 0; x < columns; x++ ) {
			for ( int y = 0; y < rows; y++ ) {
				data[y, x] = new BasisVector<T>( $"{name}.{componentNames[x + y * columns]}" );
			}
		}

		return new( data );
	}
}
