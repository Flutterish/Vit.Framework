using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Store : Instruction {
	public Store ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public uint PointerId;
	public uint ObjectId;
	public MemoryOperands? MemoryOperands;

	public override void Execute ( RuntimeScope scope ) {
		var to = ((PointerVariable)scope.Variables[PointerId]).Address!;
		var from = scope.Variables[ObjectId];

		to.Value = from.Value;
	}

	public override string ToString () {
		return $"*({GetAssignable( PointerId )}) = {GetValue( ObjectId )}{( MemoryOperands == null ? "" : $" {MemoryOperands}" )}";
	}
}
