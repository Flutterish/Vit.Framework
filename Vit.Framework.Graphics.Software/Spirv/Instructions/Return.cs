using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Return : Instruction {
	public Return ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public override void Execute ( RuntimeScope scope ) {
		scope.CodePointer = int.MaxValue;
	}

	public override string ToString () {
		return "return";
	}
}
