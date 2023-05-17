namespace Vit.Framework.Memory;

public ref struct SpanSlice<T> {
	public required Span<T> Source;
	public int Start;
	public int Length;

	public Span<T> AsSpan () => Source.Slice( Start, Length );

	public static implicit operator Span<T> ( SpanSlice<T> slice )
		=> slice.AsSpan();

	public Span<T>.Enumerator GetEnumerator () => AsSpan().GetEnumerator();

	public ref T this[Index i] => ref AsSpan()[i];
}
