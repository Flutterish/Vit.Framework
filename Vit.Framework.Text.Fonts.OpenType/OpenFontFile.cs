using Vit.Framework.Parsing.Binary;
using Vit.Framework.Text.Fonts.OpenType.Tables;

namespace Vit.Framework.Text.Fonts.OpenType;

public struct OpenFontFile {
	public Tag SfntVersion;
	public ushort TableCount;
	public ushort SearchRange;
	public ushort EntrySelector;
	public ushort RangeShift;

	[Size(nameof(TableCount))]
	public TableRecord[] TableRecords;
}

public struct TableRecord {
	public Tag TableTag;
	public uint Chacksum;
	public Offset32 Offset;
	public uint Length;
	
	[DataOffset(nameof(Offset))]
	[TypeSelector(nameof(selectType))]
	public Table? Table;

	static Type? selectType ( Tag tableTag ) {
		if ( tableTag == "cmap" )
			return typeof( CmapTable );
		else if ( tableTag == "head" )
			return typeof( HeadTable );
		else if ( tableTag == "hhea" )
			return typeof( HorizontalHeaderTable );
		else if ( tableTag == "hmtx" )
			return typeof( HorizontalMetricsTable );
		else if ( tableTag == "maxp" )
			return typeof( MaximumProfileTable );
		else if ( tableTag == "name" )
			return typeof( NamingTable );
		else if ( tableTag == "OS/2" )
			return typeof( Os2Table );
		else if ( tableTag == "kern" )
			return typeof( KeringTable );
		else if ( tableTag == "post" )
			return typeof( PostScriptTable );
		else if ( tableTag == "CFF " )
			return typeof( CffTable );
		//else if ( tableTag == "GPOS" )
		//	return typeof( GlyphPositioningTable );

		return null;
	}
}