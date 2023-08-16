using Vit.Framework.Input.Events;

namespace Vit.Framework.TwoD.UI.Input.Events;

public record UITextInputEvent : UIEvent, INonPropagableEvent {
	public required string Text { get; init; }
}
