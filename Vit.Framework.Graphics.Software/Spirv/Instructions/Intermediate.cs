using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Types;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Intermediate : CompilerObject, IValue, IAssignable {
	public Intermediate ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint TypeId;

	public DataType Type => GetDataType( TypeId );

	public override string ToString () {
		return $"let_{Id} : {GetDataType(TypeId)}";
	}
}
