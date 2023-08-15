using Vit.Framework.Input;
using Vit.Framework.Input.Events;

namespace Vit.Framework.TwoD.Input.Events;

public abstract record ClipboardEvent : UIEvent, INonPropagableEvent {
	public required Clipboard Clipboard { get; init; }
}


public record ClipboardCopyEvent : ClipboardEvent {

}

public record ClipboardPasteTextEvent : ClipboardEvent {
	public required string Text { get; init; }
}
