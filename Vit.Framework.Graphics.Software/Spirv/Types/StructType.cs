using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class StructType : DataType {
	public StructType ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint[] MemberTypeIds = Array.Empty<uint>();

	public IEnumerable<DataType> MemberTypes => MemberTypeIds.Select( GetDataType );
	public IReadOnlyDictionary<DecorationName, Decoration> GetMemberDecorations ( uint index ) => Compiler.MemberDecorations.TryGetValue( (Id, index), out var decorations )
		? decorations
		: emptyDecorations;

	protected override IRuntimeType CreateRuntimeType () {
		return new RuntimeStructType( MemberTypes.Select( x => x.GetRuntimeType() ).ToArray() );
	}

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
