namespace Vit.Framework.Graphics.Software.Spirv.Metadata;

public class ExecutionMode : CompilerObject {
	public ExecutionMode ( SpirvCompiler compiler ) : base( compiler ) { }

	public uint EntryPointId;
	public ExecutionModeType Type;
	public uint[] Data = Array.Empty<uint>();

	public override string ToString () {
		return $"Mode {Type} for {GetEntryPoint(EntryPointId)}";
	}
}
