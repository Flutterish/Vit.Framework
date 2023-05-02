namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class VoidType : DataType {
	public VoidType ( SpirvCompiler compiler ) : base( compiler ) { }

	public override string ToString () {
		return "void";
	}
}
