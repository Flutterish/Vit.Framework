namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class VectorType : DataType {
	public VectorType ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint ComponentTypeId;
	public uint Count;

	public DataType ComponentType => GetDataType( ComponentTypeId );

	public override string ToString () {
		return $"{GetDataType(ComponentTypeId)}<{Count}>";
	}
}
