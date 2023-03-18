using System.Runtime.CompilerServices;

namespace Vit.Framework.Allocation;

public readonly ref struct SpanView<T> {
	readonly Span<T> source;
	readonly int stride;
	public readonly int Length;

	public SpanView ( Span<T> source, int stride ) {
		this.source = source;
		this.stride = stride;
		Length = source.Length / stride;
	}

	public ref T this[int i] => ref source[i * stride];

	public Enumerator GetEnumerator () => new( this );

	public ref struct Enumerator {
		int index;
		SpanView<T> view;

		public Enumerator ( SpanView<T> view ) {
			index = -1;
			this.view = view;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool MoveNext () {
			index++;
			return index < view.Length;
		}

		public ref T Current {
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get => ref view[index];
		}
	}
}
