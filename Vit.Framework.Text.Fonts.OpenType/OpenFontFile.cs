﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Parsing.Binary;
using Vit.Framework.Parsing.Binary.Views;
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
}

public struct TableRecord {
	public Tag TableTag;
	public uint Chacksum;
	public Offset32 Offset;
	public uint Length;

	[DataOffset( nameof( Offset ) )]
	[TypeSelector( nameof( selectType ) )]
	public BinaryView<Table?> Table;

	static Type? selectType ( Tag tableTag ) {
		if ( tableTag == "cmap" )
			return typeof( CmapTable_old );
		else if ( tableTag == "head" )
			return typeof( HeadTable );
		else if ( tableTag == "hhea" )
			return typeof( HorizontalHeaderTable_old );
		else if ( tableTag == "hmtx" )
			return typeof( HorizontalMetricsTable_old );
		else if ( tableTag == "maxp" )
			return typeof( MaximumProfileTable_old );
		else if ( tableTag == "name" )
			return typeof( NamingTable_old );
		else if ( tableTag == "OS/2" )
			return typeof( Os2Table_old );
		else if ( tableTag == "kern" )
			return typeof( KeringTable_old );
		else if ( tableTag == "post" )
			return typeof( PostScriptTable_old );
		else if ( tableTag == "CFF " )
			return typeof( CffTable_old );
		else if ( tableTag == "glyf" )
			return typeof( GlyphDataTable_old );
		else if ( tableTag == "loca" )
			return typeof( IndexToLocationTable_old );
		//else if ( tableTag == "GPOS" )
		//	return typeof( GlyphPositioningTable );

		return null;
	}
}