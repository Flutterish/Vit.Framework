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

	//public override void Execute ( RuntimeScope scope, ShaderMemory memory ) {
	//	Debug.Assert( IndiceIds.Length == 1 );

	//	var index = memory.Read<int>( scope.VariableInfo[IndiceIds[0]].Address );
	//	var to = scope.VariableInfo[ResultId];

	//	var from = scope.VariableInfo[BaseId];
	//	var baseAddress = memory.Read<int>( from.Address );
	//	var memberOffset = ( (ICompositeRuntimeType)( (RuntimePointerType)from.Type ).Base ).GetMemberOffset( index );

	//	memory.Write( to.Address, value: baseAddress + memberOffset );
	//}

	JitedVariable index0;
	JitedVariable result;
	JitedVariable @base;
	ICompositeRuntimeType offsetSource = null!; // TODO JIT this away
	protected override void JitCompile ( RuntimeScope scope, int stackPointer ) {
		Debug.Assert( IndiceIds.Length == 1 );

		index0 = JitVariable( IndiceIds[0], scope, stackPointer );
		result = JitVariable( ResultId, scope, stackPointer );
		@base = JitVariable( BaseId, scope, stackPointer );
		offsetSource = (ICompositeRuntimeType)( (RuntimePointerType)scope.VariableInfo[BaseId].Type ).Base;
	}

	protected override void ExecuteCompiled ( ShaderOpaques opaques, ShaderMemory memory ) {
		var index = index0.Read<int>( memory );

		var baseAddress = memory.Read<int>( @base.Address( memory.StackPointer ) );
		var memberOffset = offsetSource.GetMemberOffset( index );

		memory.Write( result.Address( memory.StackPointer ), value: baseAddress + memberOffset );
	}

	public override string ToString () {
		return $"{GetAssignable(ResultId)} = &({GetValue(BaseId)}){string.Join("", IndiceIds.Select( x => $"[{GetValue(x)}]" ))}";
	}
}
