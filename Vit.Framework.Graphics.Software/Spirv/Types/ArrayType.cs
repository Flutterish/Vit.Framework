namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class ArrayType : DataType {
	public ArrayType ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint ElementTypeId;
	public uint Length;

	public DataType ElementType => GetDataType( ElementTypeId );

	public override string ToString () {
		return $"{GetDataType(ElementTypeId)}[{Length}]";
	}
}
