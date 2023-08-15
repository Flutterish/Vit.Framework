namespace Vit.Framework.TwoD.Input.Events;

public record TextInputEvent : UIEvent {
	public required string Text { get; init; }
}
