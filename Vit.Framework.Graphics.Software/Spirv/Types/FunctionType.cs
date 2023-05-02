namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class FunctionType : DataType {
	public FunctionType ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint ReturnTypeId;
	public uint[] ParameterTypeIds = Array.Empty<uint>();

	public string ArgsString => string.Join( ", ", ParameterTypeIds.Select( GetDataType ) );

	public override string ToString () {
		return $"function ({ArgsString}) -> {GetDataType(ReturnTypeId)}";
	}
}
