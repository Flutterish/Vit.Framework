using System.Text;

namespace Vit.Framework.Graphics.Software.Spirv.Runtime;

public interface ICompositeRuntimeType {
	int GetMemberOffset ( int index );
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

	public object Parse ( ReadOnlySpan<byte> data ) {
		StringBuilder sb = new();
		sb.Append( '[' );
		for ( int i = 0; i < Count; i++ ) {
			var el = ElementType.Parse( data[..ElementType.Size] );
			data = data[ElementType.Size..];
			sb.Append( el.ToString() );
			if ( i + 1 < Count )
				sb.Append( ", " );
		}
		sb.Append( ']' );

		return sb.ToString();
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
		return $"{{{string.Join( ", ", Members.AsEnumerable() )}}}";
	}

	public int GetMemberOffset ( int index ) {
		return Members.Take( index ).Sum( x => x.Size ); // TODO this works until there is padding involved
	}

	public object Parse ( ReadOnlySpan<byte> data ) {
		StringBuilder sb = new();
		sb.Append( '{' );
		for ( int i = 0; i < Members.Length; i++ ) {
			var member = Members[i];
			var el = member.Parse( data[..member.Size] );
			data = data[member.Size..];
			sb.Append( el.ToString() );
			if ( i + 1 < Members.Length )
				sb.Append( ", " );
		}
		sb.Append( '}' );

		return sb.ToString();
	}
}