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

	public T GetDeterminant () {
		var size = int.Max( Rows, Columns );
		if ( size == 1 )
			return this[0, 0];

		if ( size == 2 ) {
			return this[0, 0] * this[1, 1] - this[0, 1] * this[1, 0];
		}

		var sum = T.AdditiveIdentity;
		using var submatrix = new RentedArray<T>( ( size - 1 ) * ( size - 1 ) );
		for ( int column = 0; column < size; column++ ) {
			var multiple = column % 2 == 0 ? this[column, 0] : -this[column, 0];
			for ( int y = 1; y < size; y++ ) {
				for ( int x = 0; x < size - 1; x++ ) {
					submatrix[(y - 1) * (size - 1) + x] = this[ x >= column ? x + 1 : x, y ];
				}
			}
			var mat = new Matrix<T>( new Span2D<T>( submatrix, size - 1, size - 1 ) );
			sum += mat.GetDeterminant() * multiple;
		}
		return sum;
	}

	public Matrix<T> GetMinors () {
		using var result = new RentedArray<T>( Rows * Columns );
		using var submatrix = new RentedArray<T>( ( Rows - 1 ) * ( Columns - 1 ) );
		for ( int y = 0; y < Rows; y++ ) {
			for ( int x = 0; x < Columns; x++ ) {
				for ( int subY = 0; subY < Rows; subY++ ) {
					if ( subY == y )
						continue;

					for ( int subX = 0; subX < Columns; subX++ ) {
						if ( subX == x )
							continue;

						submatrix[( subY > y ? subY - 1 : subY ) * ( Columns - 1 ) + ( subX > x ? subX - 1 : subX )]
							= this[subX, subY];
					}
				}

				result[y * Columns + x] = new Matrix<T>( new Span2D<T>( submatrix.AsSpan(), Columns - 1, Rows - 1 ) ).GetDeterminant();
			}
		}

		return new Matrix<T>( new Span2D<T>( result, Columns, Rows ) );
	}

	public Matrix<T> CofactorCheckerboard () {
		using var result = new RentedArray<T>( Rows * Columns );
		for ( int y = 0; y < Rows; y++ ) {
			for ( int x = 0; x < Columns; x++ ) {
				result[y * Columns + x] = ((x + y) % 2 == 0) ? this[x, y] : -this[x, y];
			}
		}

		return new Matrix<T>( new Span2D<T>( result, Columns, Rows ) );
	}

	public Matrix<T> GetInverse () {
		var det = T.MultiplicativeIdentity / GetDeterminant();
		var m = GetMinors().CofactorCheckerboard();
		using var data = new RentedArray<T>( Rows * Columns );
		for ( int y = 0; y < Rows; y++ ) {
			for ( int x = 0; x < Columns; x++ ) {
				data[x + y * Columns] = m[x, y] * det;
			}
		}

		return new Matrix<T>( new Span2D<T>( data, Columns, Rows ) ).Transposed();
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
		for ( int y = 0; y < rows; y++ ) {
			for ( int x = 0; x < columns; x++ ) {
				data[y, x] = new BasisVector<T>( $"{name}[{x},{y}]" );
			}
		}

		return new( data );
	}

	public static Matrix<MultiVector<T>> GenerateLabelMatrix ( string? name, int columns, int rows, IReadOnlyList<string> componentNames ) {
		var data = new MultiVector<T>[rows, columns];
		for ( int y = 0; y < rows; y++ ) {
			for ( int x = 0; x < columns; x++ ) {
				if ( name is null )
					data[y, x] = new BasisVector<T>( $"{componentNames[x + y * columns]}" );
				else
					data[y, x] = new BasisVector<T>( $"{name}.{componentNames[x + y * columns]}" );
			}
		}
		
		return new( data );
	}
}
