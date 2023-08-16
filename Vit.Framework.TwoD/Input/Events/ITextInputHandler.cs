using Vit.Framework.Input.Events;

namespace Vit.Framework.TwoD.Input.Events;

public interface ITextInputHandler : IEventHandler<UITextInputEvent>, IEventHandler<ClipboardCopyEvent>, IEventHandler<ClipboardCutEvent>, IEventHandler<ClipboardPasteTextEvent> {
	bool OnTextInput ( UITextInputEvent @event );
	bool OnClipboardCopy ( ClipboardCopyEvent @event );
	bool OnClipboardCut ( ClipboardCutEvent @event );
	bool OnClipboardPaste ( ClipboardPasteTextEvent @event );

	bool IEventHandler<UITextInputEvent>.OnEvent ( UITextInputEvent @event ) {
		return OnTextInput( @event );
	}
	bool IEventHandler<ClipboardCopyEvent>.OnEvent ( ClipboardCopyEvent @event ) {
		return OnClipboardCopy( @event );
	}
	bool IEventHandler<ClipboardCutEvent>.OnEvent ( ClipboardCutEvent @event ) {
		return OnClipboardCut( @event );
	}
	bool IEventHandler<ClipboardPasteTextEvent>.OnEvent ( ClipboardPasteTextEvent @event ) {
		return OnClipboardPaste( @event );
	}
}
