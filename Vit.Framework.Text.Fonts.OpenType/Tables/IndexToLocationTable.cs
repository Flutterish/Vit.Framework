using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

[ParserDependency]
[ParsingDependsOn(typeof(HeadTable), typeof(MaximumProfileTable))]
public class IndexToLocationTable : Table {
	[Size(nameof(getShortSize))]
	public Offset16[] ShortOffsets = null!;
	[Size(nameof(getLongSize))]
	public Offset32[] LongOffsets = null!;

	static int getShortSize ( MaximumProfileTable maxp, HeadTable head ) {
		return head.IndexToLocFormat == 0 ? (maxp.GlyphCount + 1) : 0;
	}
	static int getLongSize ( MaximumProfileTable maxp, HeadTable head ) {
		return head.IndexToLocFormat == 1 ? ( maxp.GlyphCount + 1 ) : 0;
	}

	public IEnumerable<Offset32> Offsets => ShortOffsets.Any()
		? ShortOffsets.Select( x => new Offset32 { Value = x.Value } )
		: LongOffsets;
}
