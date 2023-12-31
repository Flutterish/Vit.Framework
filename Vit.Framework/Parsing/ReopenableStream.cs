﻿namespace Vit.Framework.Parsing;

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