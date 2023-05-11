using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Store : Instruction {
	public Store ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public uint PointerId;
	public uint ObjectId;
	public MemoryOperands? MemoryOperands;

	//public override void Execute ( RuntimeScope scope, ShaderMemory memory ) {
	//	var toPtr = scope.VariableInfo[PointerId];
	//	var toAddress = memory.Read<int>( toPtr.Address );
	//	var from = scope.VariableInfo[ObjectId];

	//	memory.Copy( from, toAddress );
	//}

	JitedVariable pointer;
	JitedVariable source;
	protected override void JitCompile ( RuntimeScope scope, int stackPointer ) {
		pointer = JitVariable( PointerId, scope, stackPointer );
		source = JitVariable( ObjectId, scope, stackPointer );
	}

	protected override void ExecuteCompiled ( ShaderOpaques opaques, ShaderMemory memory ) {
		var toAddress = memory.Read<int>( pointer.Address( memory.StackPointer ) );
		var from = source.Address( memory.StackPointer );

		memory.Copy( from, toAddress, source.Size );
	}

	public override string ToString () {
		return $"*({GetAssignable( PointerId )}) = {GetValue( ObjectId )}{( MemoryOperands == null ? "" : $" {MemoryOperands}" )}";
	}
}
