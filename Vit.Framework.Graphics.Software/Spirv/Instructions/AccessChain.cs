using System.Diagnostics;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class AccessChain : Instruction {
	public AccessChain ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint BaseId;
	public uint[] IndiceIds = Array.Empty<uint>();

	public override void Execute ( RuntimeScope scope ) {
		Debug.Assert( IndiceIds.Length == 1 );

		var index = (IVariable<int>)scope.Variables[IndiceIds[0]];
		var to = (PointerVariable)scope.Variables[ResultId];
		var from = (ICompositeVariable)((PointerVariable)scope.Variables[BaseId]).Address!;

		to.Address = from[(uint)index.Value];
	}

	public override string ToString () {
		return $"{GetAssignable(ResultId)} = &({GetValue(BaseId)}){string.Join("", IndiceIds.Select( x => $"[{GetValue(x)}]" ))}";
	}
}
