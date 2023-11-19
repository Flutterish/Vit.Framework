using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

public class IndexToLocationTable : Table {
	[Size( nameof( getShortSize ) )]
	public BinaryArrayView<Offset16> ShortOffsets;
	[Size( nameof( getLongSize ) )]
	public BinaryArrayView<Offset32> LongOffsets;

	static int getShortSize ( [Resolve] MaximumProfileTable maxp, [Resolve] HeadTable head ) {
		return head.IndexToLocFormat == 0 ? ( maxp.GlyphCount + 1 ) : 0;
	}
	static int getLongSize ( [Resolve] MaximumProfileTable maxp, [Resolve] HeadTable head ) {
		return head.IndexToLocFormat == 1 ? ( maxp.GlyphCount + 1 ) : 0;
	}

	public int Length => ShortOffsets.Any() ? ShortOffsets.Length : LongOffsets.Length;

	public Offset32 this[int index] => ShortOffsets.Any() 
		? new Offset32 { Value = ShortOffsets[index].Value }
		: LongOffsets[index];
}
