using System.Diagnostics;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class CompositeExtract : Instruction {
	public CompositeExtract ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint CompositeId;
	public uint[] Indices = Array.Empty<uint>();

	//public override void Execute ( RuntimeScope scope, ShaderMemory memory ) {
	//	Debug.Assert( Indices.Length == 1 );

	//	var index = Indices[0];
	//	var to = scope.VariableInfo[ResultId];
	//	var from = scope.VariableInfo[CompositeId];
	//	var offset = ((ICompositeRuntimeType)from.Type).GetMemberOffset( (int)index );

	//	memory.Copy( from.Address + offset, to );
	//}

	JitedVariable result;
	JitedVariable composite;
	int offset;
	protected override void JitCompile ( RuntimeScope scope, int stackPointer ) {
		Debug.Assert( Indices.Length == 1 );

		composite = JitVariable( CompositeId, scope, stackPointer );
		result = JitVariable( ResultId, scope, stackPointer );

		var from = scope.VariableInfo[CompositeId];
		var type = (ICompositeRuntimeType)from.Type;
		var index = Indices[0];

		offset = type.GetMemberOffset( (int)index );
	}

	protected override void ExecuteCompiled ( ShaderOpaques opaques, ShaderMemory memory ) {
		memory.Copy( composite.Address( memory.StackPointer ) + offset, result.Address( memory.StackPointer ), result.Size );
	}

	public override string ToString () {
		return $"{GetAssignable(ResultId)} = ({GetValue(CompositeId)}){string.Join("", Indices.Select( x => $"[{x}]" ))}";
	}
}
