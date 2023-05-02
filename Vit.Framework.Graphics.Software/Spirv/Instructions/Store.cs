using Vit.Framework.Graphics.Software.Spirv.Metadata;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Store : Instruction {
	public Store ( SourceRef sourceRef ) : base( sourceRef ) { }

	public uint PointerId;
	public uint ObjectId;
	public MemoryOperands? MemoryOperands;

	public override string ToString () {
		return $"*({GetAssignable( PointerId )}) = {GetValue( ObjectId )}{( MemoryOperands == null ? "" : $" {MemoryOperands}" )}";
	}
}
