using Vit.Framework.Graphics.Software.Spirv.Metadata;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Variable : CompilerObject, IValue, IAssignable {
	public Variable ( SpirvCompiler compiler ) : base( compiler ) { }

	public uint Id;
	public uint TypeId;
	public StorageClass StorageClass;
	public uint? InitializerId;

	public override string ToString () {
		return $"{GetName(Id) ?? $"var_{Id}" } ({StorageClass}) : {GetDataType(TypeId)}{(InitializerId == null ? "" : $" = %{InitializerId}")}";
	}
}
