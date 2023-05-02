using Vit.Framework.Graphics.Software.Spirv.Metadata;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Intermediate : CompilerObject, IValue, IAssignable {
	public Intermediate ( SpirvCompiler compiler ) : base( compiler ) { }

	public uint Id;
	public uint TypeId;

	public override string ToString () {
		return $"let_{Id} : {GetDataType(TypeId)}";
	}
}
