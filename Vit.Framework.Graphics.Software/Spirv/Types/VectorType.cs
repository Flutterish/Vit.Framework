namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class VectorType : DataType {
	public VectorType ( SpirvCompiler compiler ) : base( compiler ) { }

	public uint ComponentTypeId;
	public uint Count;

	public override string ToString () {
		return $"{GetDataType(ComponentTypeId)}<{Count}>";
	}
}
