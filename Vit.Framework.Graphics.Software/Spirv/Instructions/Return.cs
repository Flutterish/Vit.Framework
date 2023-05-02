namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Return : Instruction {
	public Return ( SourceRef sourceRef ) : base( sourceRef ) { }

	public override string ToString () {
		return "return";
	}
}
