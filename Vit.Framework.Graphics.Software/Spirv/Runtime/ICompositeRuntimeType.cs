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
}