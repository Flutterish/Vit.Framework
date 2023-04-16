using System.Collections;
using System.Text;
using Vit.Framework.Memory;
using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Adobe;

public struct Index<T> : IEnumerable<T> {
	public ushort Count;
	[Cache]
	public OffsetSize OffsetSize;
	[Size( nameof( getCount ) )]
	[Stride(nameof(getStride))]
	public BinaryArrayView<Offset> Offsets;
	BinaryViewContext context;

	[Size( nameof( getSize ) )]
	BinaryArrayView<byte> data;

	static int getCount ( ushort count ) {
		return count + 1;
	}

	static int getStride ( OffsetSize offsetSize ) {
		return offsetSize.Size;
	}

	static int getSize ( BinaryArrayView<Offset> offsets ) {
		return (int)(offsets[offsets.Length - 1] - offsets[0]);
	}

	public T this[int index] {
		get {
			var start = Offsets[index];
			var end = Offsets[index + 1];

			var length = end - start;
			var dataStart = context.Offset + start - 1;
			if ( typeof(T) == typeof(string) ) {
				using var array = new RentedArray<byte>( (int)length );
				context.StreamPosition = dataStart;
				context.Reader.Value.Stream.Read( array.AsSpan() );
				return (T)(object)Encoding.UTF8.GetString( array.AsSpan() );
			}

			var ctx = context;
			ctx.Offset = dataStart;
			ctx.Dependenies = new BinaryViewContext.Dependency() { Parent = context.Dependenies };
			ctx.CacheDependency( (int)length );
			return BinaryView<T>.Parse( ctx );
		}
	}

	public IEnumerator<T> GetEnumerator () {
		for ( int i = 0; i < Count; i++ ) {
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator () {
		for ( int i = 0; i < Count; i++ ) {
			yield return this[i];
		}
	}
}
