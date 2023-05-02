using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class VectorType : DataType {
	public VectorType ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint ComponentTypeId;
	public uint Count;

	public DataType ComponentType => GetDataType( ComponentTypeId );

	protected override IRuntimeType CreateRuntimeType () {
		return ComponentType.GetRuntimeType().Vectorize( Count );
	}

	public override string ToString () {
		return $"{GetDataType(ComponentTypeId)}<{Count}>";
	}
}
