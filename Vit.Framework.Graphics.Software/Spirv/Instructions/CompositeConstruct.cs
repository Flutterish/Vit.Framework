using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class CompositeConstruct : Instruction {
	public CompositeConstruct ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint[] ValueIds = Array.Empty<uint>();

	//public override void Execute ( RuntimeScope scope, ShaderMemory memory ) {
	//	var to = scope.VariableInfo[ResultId];
	//	var composite = (ICompositeRuntimeType)to.Type;

	//	for ( int i = 0; i < ValueIds.Length; i++ ) {
	//		memory.Copy( scope.VariableInfo[ValueIds[i]], to.Address + composite.GetMemberOffset( i ) );
	//	}
	//}

	JitedVariable result;
	JitedVariable[] values = null!;
	int[] offsets = null!;
	protected override void JitCompile ( RuntimeScope scope, int stackPointer ) {
		result = JitVariable( ResultId, scope, stackPointer );

		var to = scope.VariableInfo[ResultId];
		var composite = (ICompositeRuntimeType)to.Type;

		offsets = new int[ValueIds.Length];
		values = new JitedVariable[ValueIds.Length];
		for ( int i = 0; i < ValueIds.Length; i++ ) {
			values[i] = JitVariable( ValueIds[i], scope, stackPointer );
			offsets[i] = composite.GetMemberOffset( i );
		}
	}

	protected override void ExecuteCompiled ( ShaderOpaques opaques, ShaderMemory memory ) {
		var resultAddress = result.Address( memory.StackPointer );
		for ( int i = 0; i < values.Length; i++ ) {
			var value = values[i];
			var offset = offsets[i];

			value.CopyTo( resultAddress + offset, memory );
		}
	}

	public override string ToString () {
		return $"{GetValue(ResultId)} = <{string.Join(", ", ValueIds.Select( GetValue ))}>";
	}
}
