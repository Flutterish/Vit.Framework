using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class MatrixType : DataType {
	public MatrixType ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint ColumnTypeId;
	public uint Columns;

	public VectorType ColumnType => (VectorType)GetDataType( ColumnTypeId );
	public DataType ComponentType => ColumnType.ComponentType;

	protected override IRuntimeType CreateRuntimeType () {
		return ComponentType.GetRuntimeType().Matrixize( ColumnType.Count, Columns );
	}

	public override string ToString () {
		return $"{ComponentType}<{Columns}x{ColumnType.Count}>";
	}
}
