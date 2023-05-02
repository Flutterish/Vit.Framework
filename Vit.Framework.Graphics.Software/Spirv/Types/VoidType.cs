namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class VoidType : DataType {
	public VoidType ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public override string ToString () {
		return "void";
	}
}
