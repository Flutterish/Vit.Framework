using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Load : Instruction {
	public Load ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint PointerId;
	public MemoryOperands? MemoryOperands;

	public override void Execute ( RuntimeScope scope, ShaderMemory memory ) {
		var fromPtr = scope.VariableInfo[PointerId];
		var fromAddress = memory.Read<int>( fromPtr.Address );
		var to = scope.VariableInfo[ResultId];

		memory.Copy( fromAddress, to );
	}

	public override string ToString () {
		return $"{GetAssignable(ResultId)} = *({GetValue(PointerId)}){(MemoryOperands == null ? "" : $" {MemoryOperands}")}";
	}
}
