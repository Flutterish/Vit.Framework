using System.Text;
using Vit.Framework.Memory;
using Vit.Framework.Parsing;
using Vit.Framework.Parsing.Binary;
using Vit.Framework.Text.Fonts.OpenType.Tables;

namespace Vit.Framework.Text.Fonts.OpenType;

public class OpenTypeFont : Font {
	Ref<EndianCorrectingBinaryReader?> readerRef = new( null );
	ReopenableStream source;

	OpenFontFile header;

	public OpenTypeFont ( ReopenableStream source ) {
		this.source = source;
		using var _ = open();

		header = BinaryView<OpenFontFile>.Parse( new BinaryViewContext { Reader = readerRef! } );

		var head = header.GetTable<HeadTable>( "head" )!;
		UnitsPerEm = head.UnitsPerEm;

		var name = header.GetTable<NamingTable>( "name" )!;
		foreach ( var i in name.NameRecords ) {
			if ( i.NameId == 4 ) {
				Name = i.ToString();
				break;
			}
		}

		var maxp = header.GetTable<MaximumProfileTable>( "maxp" )!;
		var hhea = header.GetTable<HorizontalHeaderTable>( "hhea" )!;
		var hmtx = header.GetTableRecord( "hmtx" )!.Value;
		hmtx.Table.Context.CacheDependency( maxp );
		hmtx.Table.Context.CacheDependency( hhea );

		if ( header.SfntVersion == "OTTO" ) {
			// TODO CFF
		}
		else {
			var loca = header.GetTableRecord( "loca" )!.Value;
			loca.Table.Context.CacheDependency( head );
			loca.Table.Context.CacheDependency( maxp );

			var glyf = header.GetTableRecord( "glyf" )!.Value;
			glyf.Table.Context.CacheDependency( (IndexToLocationTable)loca.Table.Value! );
		}
	}

	protected override void TryLoadGlyphFor ( Rune rune ) {
		using var _ = open();
		const int lookupRange = 32;

		var rangeStart = checked( rune.Value - lookupRange );
		var rangeEnd = checked( rune.Value + lookupRange);

		var startChar = new Rune( rangeStart );
		var endChar = new Rune( rangeEnd - 1 );

		var cmap = header.GetTable<CharacterToGlyphIdTable>( "cmap" )!;
		var hmtx = header.GetTable<HorizontalMetricsTable>( "hmtx" )!;
		foreach ( var i in cmap.EncodingRecords ) {
			var sub = i.Subtable.Value;
			var encoding = EncodingTypeExtensions.GetEncodingType( i.PlatformID, i.EncodingID );
			foreach ( var (rune2, id) in sub.Enumerate( encoding, rangeStart, rangeEnd ) ) {
				if ( !GlyphsById.ContainsKey( id ) ) {
					var glyph = GetGlyph( id );

					var metric = hmtx.HorizontalMetrics[(int)id.Id];
					glyph.HorizontalAdvance = metric.AdvanceWidth;
					glyph.MinX = metric.LeftSideBearing;
					// TODO spare LSB

					if ( header.SfntVersion == "OTTO" ) {
						// TODO CFF
					}
					else {
						loadGlyphDataOutline( glyph );
					}
				}

				AddGlyphMapping( rune2, id );	
			}
		}

		for ( int i = rangeStart; i < rangeEnd; i++ ) {
			if ( !IsRuneRegistered( new Rune( i ) ) )
				AddGlyphMapping( new Rune( i ), new GlyphId( 0 ) );
		}
	}

	void loadGlyphDataOutline ( Glyph glyph ) {
		var glyf = header.GetTable<GlyphDataTable>( "glyf" )!;
		var glyphData = glyf.GetGlyph( glyph.Id );
		if ( glyphData == null )
			return;

		glyphData.CopyOutline( glyph.Outline );
		glyph.MinX = glyphData.MinX;
		glyph.MinY = glyphData.MinY;
		glyph.MaxX = glyphData.MaxX;
		glyph.MaxY = glyphData.MaxY;
	}

	DisposeAction<(OpenTypeFont self, bool wasOpen)> open () {
		bool wasOpen = source.IsOpen;
		var reader = new EndianCorrectingBinaryReader( source.Open(), isLitteEndian: false );
		readerRef.Value = reader;

		return new DisposeAction<(OpenTypeFont self, bool wasOpen)>( (this, wasOpen), static data => {
			data.self.readerRef.Value = null;
			if ( !data.wasOpen )
				data.self.source.Close();
		} );
	}
}
