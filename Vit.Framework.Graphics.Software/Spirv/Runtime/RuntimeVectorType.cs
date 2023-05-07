using System.Numerics;
using System.Text;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Software.Spirv.Runtime;

public interface IRuntimeVectorType : IRuntimeType {
	IRuntimeType ComponentType { get; }
	uint Length { get; }
}

public abstract class RuntimeVectorType<T, TVector> : RuntimeType<TVector>, ICompositeRuntimeType, 
	IInterpolatableRuntimeType, IRuntimeType, IRuntimeVectorType where T : unmanaged, INumber<T> where TVector : unmanaged 
{
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

	object IRuntimeType.Parse ( ReadOnlySpan<byte> data ) {
		StringBuilder sb = new();
		sb.Append( '<' );
		for ( int i = 0; i < Length; i++ ) {
			var el = ElementType.Parse( data[..ElementType.Size] );
			data = data[ElementType.Size..];
			sb.Append( el.ToString() );
			if ( i + 1 < Length )
				sb.Append( ", " );
		}
		sb.Append( '>' );

		return sb.ToString();
	}

	public void Interpolate ( float a, float b, float c, VariableInfo A, VariableInfo B, VariableInfo C, VariableInfo result, ShaderMemory memory ) {
		for ( int i = 0; i < Length; i++ ) {
			var offset = GetMemberOffset( i );
			var _A = new VariableInfo { Address = A.Address + offset, Type = ElementType };
			var _B = new VariableInfo { Address = B.Address + offset, Type = ElementType };
			var _C = new VariableInfo { Address = C.Address + offset, Type = ElementType };
			var _R = new VariableInfo { Address = result.Address + offset, Type = ElementType };
			( (IInterpolatableRuntimeType)ElementType ).Interpolate( a, b, c, _A, _B, _C, _R, memory );
		}
	}

	public IRuntimeType ComponentType => ElementType;
	uint IRuntimeVectorType.Length => (uint)Length;
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