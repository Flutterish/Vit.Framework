using System.Numerics;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Software.Spirv.Runtime;

public interface IMatrixType {
	void Multiply ( VariableInfo matrix, VariableInfo vector, VariableInfo result, ShaderMemory memory );
}

public abstract class RuntimeMatrixType<T, TMatrix> : RuntimeType<TMatrix>, IMatrixType where T : unmanaged, INumber<T> where TMatrix : unmanaged {
	public readonly IRuntimeType<T> ElementType;
	public RuntimeMatrixType ( IRuntimeType<T> elementType ) {
		ElementType = elementType;
	}

	public override IRuntimeType Vectorize ( uint count ) {
		throw new InvalidOperationException( "Cannot vectorize a matrix" );
	}

	public override IRuntimeType Matrixize ( uint rows, uint columns ) {
		throw new NotImplementedException();
	}

	public abstract void Multiply ( VariableInfo matrix, VariableInfo vector, VariableInfo result, ShaderMemory memory );
}

public class RuntimeMatrix4Type<T> : RuntimeMatrixType<T, Matrix4<T>> where T : unmanaged, INumber<T> {
	public RuntimeMatrix4Type ( IRuntimeType<T> elementType ) : base( elementType ) { }

	public override string ToString () {
		return $"Matrix4<{ElementType}>";
	}

	public override void Multiply ( VariableInfo matrix, VariableInfo vector, VariableInfo result, ShaderMemory memory ) {
		var mat = memory.Read<Matrix4<T>>( matrix.Address );
		var vec = memory.Read<Vector4<T>>( vector.Address );
		memory.Write( result.Address, vec * mat );
	}
}