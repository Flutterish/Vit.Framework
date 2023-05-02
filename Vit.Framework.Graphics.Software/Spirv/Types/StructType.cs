namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class StructType : DataType {
	public StructType ( SpirvCompiler compiler ) : base( compiler ) { }

	public uint TypeId;
	public uint[] MemberTypeIds = Array.Empty<uint>();

	public override string ToString () {
		return $"{tryPadRight( GetName( TypeId))}{{{string.Join(", ", MemberTypeIds.Select( (x, i) => $"{GetDataType(x)}{tryPadLeft( GetMemberName( TypeId, (uint)i))}" ))}}}";
	}

	string? tryPadRight ( string? str ) {
		return str is null ? str : ( str + ' ' );
	}

	string? tryPadLeft ( string? str ) {
		return str is null ? str : ( ' ' + str );
	}
}
