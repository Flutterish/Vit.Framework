using Vit.Framework.Parsing.Binary;
using Vit.Framework.Text.Fonts.OpenType.Tables;

namespace Vit.Framework.Text.Fonts.OpenType;

public struct OpenFontFile {
	public Tag SfntVersion;
	public ushort TableCount;
	public ushort SearchRange;
	public ushort EntrySelector;
	public ushort RangeShift;

	[Size( nameof( TableCount ) )]
	public TableRecord[] TableRecords;

	public T? GetTable<T> ( Tag tag ) where T : Table {
		foreach ( var i in TableRecords ) {
			if ( i.TableTag == tag )
				return i.Table.Value as T;
		}

		return null;
	}

	public TableRecord? GetTableRecord ( Tag tag ) {
		foreach ( var i in TableRecords ) {
			if ( i.TableTag == tag )
				return i;
		}

		return null;
	}
}

public struct TableRecord {
	public Tag TableTag;
	public uint Checksum;
	public Offset32 Offset;
	public uint Length;

	[DataOffset( nameof( Offset ) )]
	[TypeSelector( nameof( selectType ) )]
	public CachedBinaryView<Table?> Table;

	public override string ToString () {
		return $"Table record for `{TableTag}`";
	}

	static Type? selectType ( Tag tableTag ) {
		if ( tableTag == "cmap" )
			return typeof( CharacterToGlyphIdTable );
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
		//else if ( tableTag == "OS/2" )
		//	return typeof( Os2Table_old );
		//else if ( tableTag == "kern" )
		//	return typeof( KeringTable_old );
		//else if ( tableTag == "post" )
		//	return typeof( PostScriptTable_old );
		else if ( tableTag == "CFF " )
			return typeof( CompactFontFormatTable );
		else if ( tableTag == "glyf" )
			return typeof( GlyphDataTable );
		else if ( tableTag == "loca" )
			return typeof( IndexToLocationTable );
		//else if ( tableTag == "GPOS" )
		//	return typeof( GlyphPositioningTable );

		return null;
	}
}