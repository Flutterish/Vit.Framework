using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Software.Shaders;

namespace Vit.Framework.Graphics.Software.Spirv.Runtime;

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

// TODO these are more like "specialised" types than "runtime" types. At some point we will want these to generate delegates/IL rather than using them directly
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