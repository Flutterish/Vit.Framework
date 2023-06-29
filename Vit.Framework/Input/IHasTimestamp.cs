namespace Vit.Framework.Input;

public interface IHasTimestamp {
	DateTime Timestamp { get; }
}

public interface IHasRelativeTimestamp {
	TimeSpan Timestamp { get; }
}