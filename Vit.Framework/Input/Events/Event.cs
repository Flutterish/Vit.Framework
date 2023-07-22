namespace Vit.Framework.Input.Events;

public abstract record Event : IHasTimestamp {
	public required DateTime Timestamp { get; init; }
}