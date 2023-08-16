using Vit.Framework.Input.Events;

namespace Vit.Framework.TwoD.Input.Events;

public abstract record UIEvent : Event {
	public required double Timestamp { get; init; }
}