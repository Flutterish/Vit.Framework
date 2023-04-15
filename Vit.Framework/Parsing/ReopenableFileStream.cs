using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
