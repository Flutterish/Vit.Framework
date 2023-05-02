namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class ArrayType : DataType {
	public ArrayType ( SpirvCompiler compiler ) : base( compiler ) { }

	public uint ElementTypeId;
	public uint Length;

	public override string ToString () {
		return $"{GetDataType(ElementTypeId)}[{Length}]";
	}
}
