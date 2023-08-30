using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Parsing.Binary;
using Vit.Framework.Text.Fonts.OpenType.Svg;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

public class SvgTable : Table {
	public ushort Version;
	public Offset32 DocumentListOffset;
	public uint Reserved;

	[DataOffset( nameof( DocumentListOffset ) )]
	public BinaryView<SvgDocumentList> DocumentList;

	public bool TryLoadGlyphOutline ( GlyphId id, Glyph glyph ) {
		return DocumentList.Value.TryLoadGlyphOutline( id, glyph );
	}
}

[OffsetAnchor]
public struct SvgDocumentList {
	public ushort NumEntries;
	[Size(nameof(NumEntries))]
	public BinaryArrayView<SvgDocumentRecord> DocumentRecords;

	public bool TryLoadGlyphOutline ( GlyphId id, Glyph glyph ) {
		foreach ( var i in DocumentRecords ) {
			if ( i.EndGlyphId < id.Value )
				continue;

			if ( i.StartGlyphId <= id.Value ) {
				i.LoadGlyphOutline( glyph );
				return true;
			}
			else {
				return false;
			}
		}

		return false;
	}
}

public struct SvgDocumentRecord {
	public ushort StartGlyphId; 
	public ushort EndGlyphId;
	public Offset32 DocumentOffset;
	public uint DocumentLength;

	[DataOffset(nameof(DocumentOffset))]
	[Size(nameof(DocumentLength))]
	public BinaryArrayView<byte> DocumentData;

	public void LoadGlyphOutline ( Glyph glyph ) {
		using var data = DocumentData.GetRented();
		if ( data.AsSpan() is [0x1F, 0x8B, 0x08, ..] ) {
			throw new NotImplementedException(); // TODO gzip compressed
		}

		Debug.Assert( Encoding.UTF8.GetString( data ).Length == data.Length );
		SvgOutline.Parse( data, glyph );
	}
}
