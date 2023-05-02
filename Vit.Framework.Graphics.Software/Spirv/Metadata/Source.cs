namespace Vit.Framework.Graphics.Software.Spirv.Metadata;

public class Source : CompilerObject {
	public Source ( SpirvCompiler compiler ) : base( compiler ) { }

	public SourceLanguage SourceLanguage;
	public uint Version;
	public uint? FileNameId;
	public string? SourceText;

	public override string ToString () {
		return $"{SourceLanguage} {Version}";
	}
}
