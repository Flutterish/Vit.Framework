using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	public Table Table;

	static Type? selectType ( Tag tag ) {
		if ( tag == "cmap" )
			return typeof( CmapTable );
		else if ( tag == "head" )
			return typeof( HeadTable );
		else if ( tag == "hhea" )
			return typeof( HorizontalHeaderTable );
		else if ( tag == "hmtx" )
			return typeof( HorizontalMetricsTable );
		else if ( tag == "maxp" )
			return typeof( MaximumProfileTable );

		return null;
	}
}