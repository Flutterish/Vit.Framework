using System.Diagnostics;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class AccessChain : Instruction {
	public AccessChain ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint BaseId;
	public uint[] IndiceIds = Array.Empty<uint>();

	public override void Execute ( RuntimeScope scope, ShaderMemory memory ) {
		Debug.Assert( IndiceIds.Length == 1 );

		var index = memory.Read<int>( scope.VariableInfo[IndiceIds[0]].Address );
		var to = scope.VariableInfo[ResultId];

		var from = scope.VariableInfo[BaseId];
		var baseAddress = memory.Read<int>( from.Address );
		var memberOffset = ((ICompositeRuntimeType)((RuntimePointerType)from.Type).Base).GetMemberOffset( index );

		memory.Write( to.Address, value: baseAddress + memberOffset );
	}

	public override string ToString () {
		return $"{GetAssignable(ResultId)} = &({GetValue(BaseId)}){string.Join("", IndiceIds.Select( x => $"[{GetValue(x)}]" ))}";
	}
}
