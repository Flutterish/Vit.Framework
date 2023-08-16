using Vit.Framework.Input.Events;
using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.Input.Events;

public abstract record UIEvent : Event {
	public required Millis Timestamp { get; init; }
}