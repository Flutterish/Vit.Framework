using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class CompositeConstruct : Instruction {
	public CompositeConstruct ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint[] Values = Array.Empty<uint>();

	public override void Execute ( RuntimeScope scope, ShaderMemory memory ) {
		var to = scope.VariableInfo[ResultId];
		var composite = (ICompositeRuntimeType)to.Type;

		for ( int i = 0; i < Values.Length; i++ ) {
			memory.Copy( scope.VariableInfo[Values[i]], to.Address + composite.GetMemberOffset( i ) );
		}
	}

	public override string ToString () {
		return $"{GetValue(ResultId)} = <{string.Join(", ", Values.Select( GetValue ))}>";
	}
}
