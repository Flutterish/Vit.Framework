using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Parsing;

public abstract class ReopenableStream {
	Stream? stream;
	protected abstract Stream OpenStream ();

	public bool IsOpen => stream != null;

	public Stream Open () {
		return stream ??= OpenStream();
	}

	public void Close () {
		stream?.Dispose();
		stream = null;
	}
}