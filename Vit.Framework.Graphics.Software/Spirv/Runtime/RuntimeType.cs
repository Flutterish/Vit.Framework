using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Software.Spirv.Runtime;

public interface IRuntimeType {
	int Size { get; }
	IRuntimeType Vectorize ( uint count );
	IRuntimeType Matrixize ( uint rows, uint columns );

	IVariable CreateVariable ();
}
public interface IRuntimeType<T> : IRuntimeType where T : unmanaged {
	static readonly int size = Marshal.SizeOf( default(T) );
	int IRuntimeType.Size => size;

	IVariable IRuntimeType.CreateVariable () => CreateVariable();
	new IVariable<T> CreateVariable ();
}

public abstract class RuntimeType<T> : IRuntimeType<T> where T : unmanaged {
	public abstract IRuntimeType Vectorize ( uint count );
	public abstract IVariable<T> CreateVariable ();
	public abstract IRuntimeType Matrixize ( uint rows, uint columns );
}

public class RuntimeNumberType<T> : RuntimeType<T> where T : unmanaged, INumber<T> {
	public override IVariable<T> CreateVariable () {
		return new NumericVariable<T>( this );
	}

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
}

public class RuntimeVector2Type<T> : RuntimeType<Vector2<T>> where T : unmanaged, INumber<T> {
	public readonly IRuntimeType<T> ElementType;
	public RuntimeVector2Type ( IRuntimeType<T> elementType ) {
		ElementType = elementType;
	}

	public override IVariable<Vector2<T>> CreateVariable () {
		return new Vector2Variable<T>( this, ElementType );
	}

	public override IRuntimeType Vectorize ( uint count ) {
		throw new InvalidOperationException( "Cannot vectorize a vector" );
	}

	public override IRuntimeType Matrixize ( uint rows, uint columns ) {
		throw new NotImplementedException();
	}

	public override string ToString () {
		return $"Vector2<{typeof( T ).Name}>";
	}
}

public class RuntimeVector3Type<T> : RuntimeType<Vector3<T>> where T : unmanaged, INumber<T> {
	public readonly IRuntimeType<T> ElementType;
	public RuntimeVector3Type ( IRuntimeType<T> elementType ) {
		ElementType = elementType;
	}

	public override IVariable<Vector3<T>> CreateVariable () {
		return new Vector3Variable<T>( this, ElementType );
	}

	public override IRuntimeType Vectorize ( uint count ) {
		throw new InvalidOperationException( "Cannot vectorize a vector" );
	}

	public override IRuntimeType Matrixize ( uint rows, uint columns ) {
		throw new NotImplementedException();
	}

	public override string ToString () {
		return $"Vector3<{typeof( T ).Name}>";
	}
}

public class RuntimeVector4Type<T> : RuntimeType<Vector4<T>> where T : unmanaged, INumber<T> {
	public readonly IRuntimeType<T> ElementType;
	public RuntimeVector4Type ( IRuntimeType<T> elementType ) {
		ElementType = elementType;
	}

	public override IVariable<Vector4<T>> CreateVariable () {
		return new Vector4Variable<T>( this, ElementType );
	}

	public override IRuntimeType Vectorize ( uint count ) {
		throw new InvalidOperationException( "Cannot vectorize a vector" );
	}

	public override IRuntimeType Matrixize ( uint rows, uint columns ) {
		throw new NotImplementedException();
	}

	public override string ToString () {
		return $"Vector4<{typeof( T ).Name}>";
	}
}

public class RuntimeMatrix4Type<T> : RuntimeType<Matrix4<T>> where T : unmanaged, INumber<T> {
	public readonly IRuntimeType<T> ElementType;
	public RuntimeMatrix4Type ( IRuntimeType<T> elementType ) {
		ElementType = elementType;
	}
	public override IVariable<Matrix4<T>> CreateVariable () {
		return new Matrix4Variable<T>( this, ElementType );
	}

	public override IRuntimeType Vectorize ( uint count ) {
		throw new InvalidOperationException( "Cannot vectorize a matrix" );
	}

	public override IRuntimeType Matrixize ( uint rows, uint columns ) {
		throw new NotImplementedException();
	}

	public override string ToString () {
		return $"Matrix4<{ElementType}>";
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

	public IVariable CreateVariable () {
		return new ArrayVariable( this );
	}

	public override string ToString () {
		return $"{ElementType}[{Count}]";
	}
}

public class RuntimeStructType : IRuntimeType {
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

	public IVariable CreateVariable () {
		return new StructVariable( this );
	}

	public override string ToString () {
		return $"{{{string.Join(", ", Members.AsEnumerable())}}}";
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

	public IVariable CreateVariable () {
		return new PointerVariable( this );
	}

	public override string ToString () {
		return $"{Base}*";
	}
}