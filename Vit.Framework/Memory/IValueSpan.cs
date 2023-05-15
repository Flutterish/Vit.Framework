namespace Vit.Framework.Memory;

public interface IValueSpan<T> : IReadOnlyValueSpan<T> {
	Span<T> AsSpan ();
}

public interface IReadOnlyValueSpan<T> {
	ReadOnlySpan<T> AsReadOnlySpan ();
}
