using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Types;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Variable : CompilerObject, IValue, IAssignable {
	public Variable ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint TypeId;
	public StorageClass StorageClass;
	public uint? InitializerId;

	public PointerType Type => (PointerType)GetDataType( TypeId );

	public override string ToString () {
		return $"{GetName(Id) ?? $"var_{Id}" } ({StorageClass}) : {GetDataType(TypeId)}{(InitializerId == null ? "" : $" = %{InitializerId}")}";
	}
}
