namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class StructType : DataType {
	public StructType ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint[] MemberTypeIds = Array.Empty<uint>();

	public IEnumerable<DataType> MemberTypes => MemberTypeIds.Select( GetDataType );

	public override string ToString () {
		return $"{tryPadRight( GetName( Id ))}{{{string.Join(", ", MemberTypeIds.Select( (x, i) => $"{GetDataType(x)}{tryPadLeft( GetMemberName( Id, (uint)i))}" ))}}}";
	}

	string? tryPadRight ( string? str ) {
		return str is null ? str : ( str + ' ' );
	}

	string? tryPadLeft ( string? str ) {
		return str is null ? str : ( ' ' + str );
	}
}
