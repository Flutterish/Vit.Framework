namespace Vit.Framework.Graphics.Software.Spirv.Metadata;

public class Decoration : CompilerObject {
	public Decoration ( SpirvCompiler compiler ) : base( compiler, uint.MaxValue ) { }

	public DecorationName Name;
	public uint[] Data = Array.Empty<uint>();

	public override string ToString () {
		return $"{Name}{(Data.Any() ? $" ({string.Join(", ", Data)})" : "")}";
	}
}
