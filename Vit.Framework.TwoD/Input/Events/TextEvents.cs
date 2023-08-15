namespace Vit.Framework.TwoD.Input.Events;

public record UITextInputEvent : UIEvent {
	public required string Text { get; init; }
}
