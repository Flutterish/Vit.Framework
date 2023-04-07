using System.Runtime.CompilerServices;

namespace Vit.Framework.Memory;

public readonly ref struct ReadOnlySpanView<T> {
	readonly ReadOnlySpan<T> source;
	readonly int stride;
	public readonly int Length;

	public ReadOnlySpanView ( ReadOnlySpan<T> source, int stride ) {
		this.source = source;
		this.stride = stride;
		Length = source.Length / stride;
	}

	public T this[int i] => source[i * stride];

	public Enumerator GetEnumerator () => new( this );

	public ref struct Enumerator {
		int index;
		ReadOnlySpanView<T> view;

		public Enumerator ( ReadOnlySpanView<T> view ) {
			index = -1;
			this.view = view;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool MoveNext () {
			index++;
			return index < view.Length;
		}

		public T Current {
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get => view[index];
		}
	}
}