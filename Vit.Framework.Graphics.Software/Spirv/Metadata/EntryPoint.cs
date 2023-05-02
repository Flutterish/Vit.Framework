namespace Vit.Framework.Graphics.Software.Spirv.Metadata;

public class EntryPoint : CompilerObject {
	public EntryPoint ( SpirvCompiler compiler ) : base( compiler ) { }

	public ExecutionModel ExecutionModel;
	public uint FunctionId;
	public string Name = string.Empty;
	public uint[] InterfaceIds = Array.Empty<uint>();

	public override string ToString () {
		return $"{ExecutionModel} Entry Point `{Name}` : {GetFunction(FunctionId)} | Interfaces: [{string.Join(", ", InterfaceIds.Select( GetVariable ))}]";
	}
}
