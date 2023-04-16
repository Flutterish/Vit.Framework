namespace Vit.Framework.Parsing;

public class ReopenableFileStream : ReopenableStream {
	public readonly string Path;

	public ReopenableFileStream ( string path ) {
		Path = path;
	}

	protected override Stream OpenStream () {
		return File.OpenRead( Path );
	}
}
