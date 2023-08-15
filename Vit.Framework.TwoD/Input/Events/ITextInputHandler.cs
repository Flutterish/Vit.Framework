using Vit.Framework.Input.Events;

namespace Vit.Framework.TwoD.Input.Events;

public interface ITextInputHandler : IEventHandler<UITextInputEvent>, IEventHandler<ClipboardCopyEvent>, IEventHandler<ClipboardPasteTextEvent>, IFocusable {
	bool OnTextInput ( UITextInputEvent @event );
	bool OnClipboardCopy ( ClipboardCopyEvent @event );
	bool OnClipboardPaste ( ClipboardPasteTextEvent @event );

	bool IEventHandler<UITextInputEvent>.OnEvent ( UITextInputEvent @event ) {
		return OnTextInput( @event );
	}
	bool IEventHandler<ClipboardCopyEvent>.OnEvent ( ClipboardCopyEvent @event ) {
		return OnClipboardCopy( @event );
	}
	bool IEventHandler<ClipboardPasteTextEvent>.OnEvent ( ClipboardPasteTextEvent @event ) {
		return OnClipboardPaste( @event );
	}
}
