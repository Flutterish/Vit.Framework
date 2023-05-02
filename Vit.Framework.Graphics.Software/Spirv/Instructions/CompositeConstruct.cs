using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class CompositeConstruct : Instruction {
	public CompositeConstruct ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint[] Values = Array.Empty<uint>();

	public override void Execute ( RuntimeScope scope ) {
		var to = (ICompositeVariable)scope.Variables[ResultId];
		for ( uint i = 0; i < Values.Length; i++ ) {
			to[i].Value = scope.Variables[Values[i]].Value;
		}
	}

	public override string ToString () {
		return $"{GetValue(ResultId)} = <{string.Join(", ", Values.Select( GetValue ))}>";
	}
}
