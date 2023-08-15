using Vit.Framework.Collections;

namespace Vit.Framework.Input;

public enum ClipboardContent {
	Text
}

public abstract class Clipboard {
	public abstract IReadOnlyList<ClipboardContent> ClipboardContents { get; }

	public abstract bool CopyText ( string value );
	public abstract string? GetText ( int index );
}

public class InMemoryClipboard : Clipboard {
	List<ClipboardContent> contents = new();
	RingBuffer<string> texts = new( 4 );
	public override IReadOnlyList<ClipboardContent> ClipboardContents => contents;

	public override bool CopyText ( string value ) {
		if ( texts.Length != texts.Capacity )
			contents.Add( ClipboardContent.Text );

		texts.Push( value );
		return true;
	}

	public override string? GetText ( int index ) {
		return texts.Peek( index + 1 );
	}
}