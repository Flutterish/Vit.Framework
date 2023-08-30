using System.Diagnostics.CodeAnalysis;
using System.Net.Security;
using Vit.Framework.Memory;
using Vit.Framework.Parsing;
using Vit.Framework.Parsing.Binary;
using Vit.Framework.Text.Fonts.OpenType.Adobe;
using Vit.Framework.Text.Fonts.OpenType.Tables;
using Vit.Framework.Text.Outlines;

namespace Vit.Framework.Text.Fonts.OpenType;

public class OpenTypeFont : Font {
	Ref<EndianCorrectingBinaryReader?> readerRef = new( null );
	ReopenableStream source;

	bool hasSvgOutlines;
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

		hasSvgOutlines = header.TableRecords.Any( x => x.TableTag == "SVG " );

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
			if ( encoding != EncodingType.Unicode ) // TODO we dont need to use all subtables, only the "best" one
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
			glyph.HorizontalAdvance = hmtx.HorizontalMetrics[^1].AdvanceWidth;
			glyph.MinX = hmtx.LeftSideBearings[(int)id.Value - hmtx.HorizontalMetrics.Length];
		}

		if ( header.SfntVersion == "OTTO" ) {
			throw new NotImplementedException(); // TODO otto bounds
		}
		else {
			var glyf = header.GetTable<GlyphDataTable>( "glyf" )!;
			var glyphData = glyf.GetHeader( glyph.Id );

			if ( glyphData == null )
				return;

			glyph.MinX = glyphData.Value.MinX;
			glyph.MinY = glyphData.Value.MinY;
			glyph.MaxX = glyphData.Value.MaxX;
			glyph.MaxY = glyphData.Value.MaxY;
		}
	}

	SplineOutline? loadGlyphDataOutline ( GlyphId glyph ) {
		var glyf = header.GetTable<GlyphDataTable>( "glyf" )!;
		var glyphData = glyf.GetGlyph( glyph );
		if ( glyphData == null )
			return null;

		var outline = new SplineOutline();
		glyphData.CopyOutline( outline, glyf );
		return outline;
	}

	SplineOutline loadCharstringOutline ( GlyphId glyph ) {
		var cff = header.GetTable<CompactFontFormatTable>( "CFF " )!.Data;
		var global = cff.GlobalSubrs;
		var local = cff.GetPrivateDict( 0 )!.Value.LocalSubrs;
		var charStrings = cff.GetCharStrings( 0 )!.Value;
		var charString = charStrings[(int)glyph.Value];
		var charset = cff.GetCharset( 0 )!.Value;
		var sid = glyph.Value == 0 ? new StringId() : charset.Glyphs[ (int)glyph.Value - 1 ];
		var name = cff.GetString( sid );

		GetGlyph( glyph ).Names.Add( name );
		return CharStringInterpreter.Load( charString, global, local );
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

	public override bool TryFetchOutline<TOutline> ( GlyphId id, [NotNullWhen( true )] out TOutline? outline ) where TOutline : default {
		using var _ = open(); // TODO open for longer
		//if ( hasSvgOutlines ) {
		//	var svg = header.GetTable<SvgTable>( "SVG " )!;
		//	if ( svg.TryLoadGlyphOutline( id, glyph ) ) {
		//		var calculated = glyph.CalculatedBoundingBox;
		//		glyph.MinX = calculated.MinX;
		//		glyph.MinY = calculated.MinY;
		//		glyph.MaxX = calculated.MaxX;
		//		glyph.MaxY = calculated.MaxY;
		//		return;
		//	}
		//}

		if ( typeof(SplineOutline).IsAssignableTo( typeof(TOutline) ) ) {
			if ( header.SfntVersion == "OTTO" ) {
				outline = (TOutline)(object)loadCharstringOutline( id );
				return true;
			}
			else {
				outline = (TOutline?)(object?)loadGlyphDataOutline( id );
				return outline != null;
			}
		}

		outline = default;
		return false;
	}
}
