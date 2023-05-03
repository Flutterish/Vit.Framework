using System.Numerics;
using System.Text;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Software.Spirv.Runtime;

public interface IMatrixType {
	void Multiply ( VariableInfo matrix, VariableInfo vector, VariableInfo result, ShaderMemory memory );
}

public abstract class RuntimeMatrixType<T, TMatrix> : RuntimeType<TMatrix>, IMatrixType, IRuntimeType where T : unmanaged, INumber<T> where TMatrix : unmanaged {
	public readonly IRuntimeType<T> ElementType;
	public readonly int Rows;
	public readonly int Columns;
	public RuntimeMatrixType ( IRuntimeType<T> elementType, int rows, int columns ) {
		ElementType = elementType;
		Rows = rows;
		Columns = columns;
	}

	public override IRuntimeType Vectorize ( uint count ) {
		throw new InvalidOperationException( "Cannot vectorize a matrix" );
	}

	public override IRuntimeType Matrixize ( uint rows, uint columns ) {
		throw new NotImplementedException();
	}

	object IRuntimeType.Parse ( ReadOnlySpan<byte> data ) {
		StringBuilder sb = new();
		sb.Append( '<' );
		for ( int y = 0; y < Rows; y++ ) {
			sb.Append( '<' );
			for ( int x = 0; x < Columns; x++ ) {
				var el = ElementType.Parse( data[..ElementType.Size] );
				data = data[ElementType.Size..];
				sb.Append( el.ToString() );

				if ( x + 1 < Columns )
					sb.Append( ", " );
			}

			sb.Append( '>' );
			if ( y + 1 < Rows )
				sb.Append( ", " );
		}
		sb.Append( '>' );

		return sb.ToString();
	}

	public abstract void Multiply ( VariableInfo matrix, VariableInfo vector, VariableInfo result, ShaderMemory memory );
}

public class RuntimeMatrix4Type<T> : RuntimeMatrixType<T, Matrix4<T>> where T : unmanaged, INumber<T> {
	public RuntimeMatrix4Type ( IRuntimeType<T> elementType ) : base( elementType, 4, 4 ) { }

	public override string ToString () {
		return $"Matrix4<{ElementType}>";
	}

	public override void Multiply ( VariableInfo matrix, VariableInfo vector, VariableInfo result, ShaderMemory memory ) {
		var mat = memory.Read<Matrix4<T>>( matrix.Address );
		var vec = memory.Read<Vector4<T>>( vector.Address );
		memory.Write( result.Address, vec * mat );
	}
}