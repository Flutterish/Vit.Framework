using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Software.Spirv.Runtime;

public interface ICompositeRuntimeType {
	int GetMemberOffset ( int index );
}

public interface IInterpolatableRuntimeType {
	void Interpolate ( float a, float b, float c, VariableInfo A, VariableInfo B, VariableInfo C, VariableInfo result, ShaderMemory memory );
}

public interface IRuntimeType {
	int Size { get; }
	IRuntimeType Vectorize ( uint count );
	IRuntimeType Matrixize ( uint rows, uint columns );
}
public interface IRuntimeType<T> : IRuntimeType where T : unmanaged {
	static readonly int size = Marshal.SizeOf( default(T) );
	int IRuntimeType.Size => size;
}

public abstract class RuntimeType<T> : IRuntimeType<T> where T : unmanaged {
	public abstract IRuntimeType Vectorize ( uint count );
	public abstract IRuntimeType Matrixize ( uint rows, uint columns );
}

public class RuntimeNumberType<T> : RuntimeType<T>, IInterpolatableRuntimeType where T : unmanaged, INumber<T> {
	public override IRuntimeType Vectorize ( uint count ) {
		if ( count == 2 ) {
			return new RuntimeVector2Type<T>( this );
		}
		else if ( count == 3 ) {
			return new RuntimeVector3Type<T>( this );
		}
		else if ( count == 4 ) {
			return new RuntimeVector4Type<T>( this );
		}

		throw new ArgumentException( $"Invalid vector size: {count}", nameof(count) );
	}

	public override IRuntimeType Matrixize ( uint rows, uint columns ) {
		if ( rows == 4 && columns == 4 ) {
			return new RuntimeMatrix4Type<T>( this );
		}

		throw new NotImplementedException();
	}

	public override string ToString () {
		return $"{typeof(T).Name}";
	}

	public void Interpolate ( float a, float b, float c, VariableInfo A, VariableInfo B, VariableInfo C, VariableInfo result, ShaderMemory memory ) {
		var _A = memory.Read<T>( A.Address );
		var _B = memory.Read<T>( B.Address );
		var _C = memory.Read<T>( C.Address );

		memory.Write( result.Address, T.CreateChecked( float.CreateTruncating( _A ) * a + float.CreateTruncating( _B ) * b + float.CreateTruncating( _C ) * c ) );
	}
}

public abstract class RuntimeVectorType<T, TVector> : RuntimeType<TVector>, ICompositeRuntimeType, IInterpolatableRuntimeType where T : unmanaged, INumber<T> where TVector : unmanaged {
	public readonly IRuntimeType<T> ElementType;
	public readonly int Length;
	public RuntimeVectorType ( IRuntimeType<T> elementType, int length ) {
		ElementType = elementType;
		Length = length;
	}

	public override IRuntimeType Vectorize ( uint count ) {
		throw new InvalidOperationException( "Cannot vectorize a vector" );
	}

	public override IRuntimeType Matrixize ( uint rows, uint columns ) {
		throw new NotImplementedException();
	}

	public int GetMemberOffset ( int index ) {
		return ElementType.Size * index;
	}

	public void Interpolate ( float a, float b, float c, VariableInfo A, VariableInfo B, VariableInfo C, VariableInfo result, ShaderMemory memory ) {
		for ( int i = 0; i < Length; i++ ) {
			var offset = GetMemberOffset( i );
			var _A = new VariableInfo { Address = A.Address + offset, Type = ElementType };
			var _B = new VariableInfo { Address = B.Address + offset, Type = ElementType };
			var _C = new VariableInfo { Address = C.Address + offset, Type = ElementType };
			var _R = new VariableInfo { Address = result.Address + offset, Type = ElementType };
			((IInterpolatableRuntimeType)ElementType).Interpolate( a, b, c, _A, _B, _C, _R, memory );
		}
	}
}

public class RuntimeVector2Type<T> : RuntimeVectorType<T, Vector2<T>> where T : unmanaged, INumber<T> {
	public RuntimeVector2Type ( IRuntimeType<T> elementType ) : base( elementType, 2 ) { }

	public override string ToString () {
		return $"Vector2<{typeof( T ).Name}>";
	}
}

public class RuntimeVector3Type<T> : RuntimeVectorType<T, Vector3<T>> where T : unmanaged, INumber<T> {
	public RuntimeVector3Type ( IRuntimeType<T> elementType ) : base( elementType, 3 ) { }

	public override string ToString () {
		return $"Vector3<{typeof( T ).Name}>";
	}
}

public class RuntimeVector4Type<T> : RuntimeVectorType<T, Vector4<T>> where T : unmanaged, INumber<T> {
	public RuntimeVector4Type ( IRuntimeType<T> elementType ) : base( elementType, 4 ) { }

	public override string ToString () {
		return $"Vector4<{typeof( T ).Name}>";
	}
}

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

public class RuntimeArrayType : IRuntimeType {
	public readonly int Count;
	public readonly IRuntimeType ElementType;
	public RuntimeArrayType ( IRuntimeType elementType, int count ) {
		Count = count;
		ElementType = elementType;
		Size = Count * ElementType.Size;
	}

	public int Size { get; }

	public IRuntimeType Vectorize ( uint count ) {
		throw new InvalidOperationException( "Cannot vectorize an array" );
	}

	public IRuntimeType Matrixize ( uint rows, uint columns ) {
		throw new NotImplementedException();
	}
	public override string ToString () {
		return $"{ElementType}[{Count}]";
	}
}

public class RuntimeStructType : IRuntimeType, ICompositeRuntimeType {
	public readonly IRuntimeType[] Members;
	public RuntimeStructType ( IRuntimeType[] members ) {
		Members = members;
		Size = Members.Sum( x => x.Size );
	}

	public int Size { get; }

	public IRuntimeType Vectorize ( uint count ) {
		throw new InvalidOperationException( "Cannot vectorize a struct" );
	}

	public IRuntimeType Matrixize ( uint rows, uint columns ) {
		throw new NotImplementedException();
	}

	public override string ToString () {
		return $"{{{string.Join(", ", Members.AsEnumerable())}}}";
	}

	public int GetMemberOffset ( int index ) {
		return Members.Take( index ).Sum( x => x.Size ); // TODO this works until there is padding involved
	}
}

public class RuntimePointerType : IRuntimeType {
	public IRuntimeType Base;
	public RuntimePointerType ( IRuntimeType @base ) {
		Base = @base;
		Size = sizeof(int);
	}

	public int Size { get; }

	public IRuntimeType Vectorize ( uint count ) {
		throw new InvalidOperationException( "Cannot vectorize a pointer" );
	}

	public IRuntimeType Matrixize ( uint rows, uint columns ) {
		throw new NotImplementedException();
	}

	public override string ToString () {
		return $"{Base}*";
	}
}