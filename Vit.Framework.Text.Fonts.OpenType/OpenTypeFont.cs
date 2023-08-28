using Vit.Framework.Memory;
using Vit.Framework.Parsing;
using Vit.Framework.Parsing.Binary;
using Vit.Framework.Text.Fonts.OpenType.Adobe;
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

		if ( header.SfntVersion != "OTTO" ) {
			var loca = header.GetTableRecord( "loca" )!.Value;
			loca.Table.Context.CacheDependency( head );
			loca.Table.Context.CacheDependency( maxp );

			var glyf = header.GetTableRecord( "glyf" )!.Value;
			glyf.Table.Context.CacheDependency( (IndexToLocationTable)loca.Table.Value! );
		}

		loadGlyphId( 0, header.GetTable<HorizontalMetricsTable>( "hmtx" )! );
	}

	protected override void TryLoadGlyphFor ( UnicodeExtendedGraphemeCluster cluster ) {
		using var _ = open();

		Span<byte> keyBytes = stackalloc byte[cluster.ByteLength];
		cluster.Bytes.CopyTo( keyBytes );
		var pageByteIndex = keyBytes.Length - 4;
		var key = new UnicodeExtendedGraphemeCluster( keyBytes );

		var cmap = header.GetTable<CharacterToGlyphIdTable>( "cmap" )!;
		var hmtx = header.GetTable<HorizontalMetricsTable>( "hmtx" )!;
		foreach ( var i in cmap.EncodingRecords ) {
			var sub = i.Subtable.Value;
			var encoding = EncodingTypeExtensions.GetEncodingType( i.PlatformID, i.EncodingID );
			if ( encoding != EncodingType.Unicode )
				continue;

			foreach ( var (lastByte, id) in sub.EnumeratePage( cluster ) ) {
				keyBytes[pageByteIndex] = lastByte;
				loadGlyphId( id, hmtx );
				AddGlyphMapping( key, id );
			}
		}

		for ( int i = 0; i < 256; i++ ) {
			keyBytes[pageByteIndex] = (byte)i;
			if ( !IsGlyphRegistered( key ) )
				AddGlyphMapping( key, 0 );
		}
	}

	void loadGlyphId ( GlyphId id, HorizontalMetricsTable hmtx ) {
		if ( GlyphsById.ContainsKey( id ) )
			return;

		var glyph = GetGlyph( id );
		if ( id.Value < (ulong)hmtx.HorizontalMetrics.Length ) {
			var metric = hmtx.HorizontalMetrics[(int)id.Value];
			glyph.HorizontalAdvance = metric.AdvanceWidth;
			glyph.MinX = metric.LeftSideBearing;
		}
		else {
			glyph.MinX = hmtx.LeftSideBearings[(int)id.Value - hmtx.HorizontalMetrics.Length];
		}

		if ( header.SfntVersion == "OTTO" ) {
			loadCharstringOutline( glyph );
		}
		else {
			loadGlyphDataOutline( glyph );
		}
	}

	void loadGlyphDataOutline ( Glyph glyph ) {
		var glyf = header.GetTable<GlyphDataTable>( "glyf" )!;
		var glyphData = glyf.GetGlyph( glyph.Id );
		if ( glyphData == null )
			return;

		glyphData.CopyOutline( glyph.Outline, glyf );
		glyph.MinX = glyphData.MinX;
		glyph.MinY = glyphData.MinY;
		glyph.MaxX = glyphData.MaxX;
		glyph.MaxY = glyphData.MaxY;
	}

	void loadCharstringOutline ( Glyph glyph ) {
		var cff = header.GetTable<CompactFontFormatTable>( "CFF " )!.Data;
		var global = cff.GlobalSubrs;
		var local = cff.GetPrivateDict( 0 )!.Value.LocalSubrs;
		var charStrings = cff.GetCharStrings( 0 )!.Value;
		var charString = charStrings[(int)glyph.Id.Value];
		var charset = cff.GetCharset( 0 )!.Value;
		var sid = glyph.Id.Value == 0 ? new StringId() : charset.Glyphs[ (int)glyph.Id.Value - 1 ];
		var name = cff.GetString( sid );

		glyph.Names.Add( name );
		CharStringInterpreter.Load( charString, glyph, global, local );
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
