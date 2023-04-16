using System.Diagnostics;
using System.Text;
using Vit.Framework.Parsing;
using Vit.Framework.Parsing.Binary;
using Vit.Framework.Text.Fonts.OpenType.Tables;

namespace Vit.Framework.Text.Fonts.OpenType;

public class OpenTypeFont_old : Font {
	public OpenFontFile_old Data;

	public OpenTypeFont_old ( OpenFontFile_old data ) {
		Data = data;

		var tablesByTag = new Dictionary<Tag, Table?>();
		foreach ( var i in data.TableRecords ) {
			tablesByTag[i.TableTag] = i.Table;
		}

		var head = (HeadTable)tablesByTag["head"]!;
		UnitsPerEm = head.UnitsPerEm;

		var cmap = (CmapTable_old)tablesByTag["cmap"]!;
		foreach ( var record in cmap.EncodingRecords ) {
			var subtable = record.Subtable;
			if ( subtable == null ) {
				Debug.Fail( "cmap subtable is null" );
				continue;
			}
			var platform = record.PlatformID;
			var encoding = record.EncodingID;
			foreach ( var (charcode, glyph) in subtable.Glyphs ) {
				AddGlyphMapping( Decode( charcode, platform, encoding ), glyph );
			}
		}

		var name = (NamingTable_old)tablesByTag["name"]!;
		foreach ( var i in name.NameRecords ) {
			if ( i.NameId == 4 ) {
				Name = i.ToString();
				break;
			}
		}

		if ( tablesByTag.TryGetValue( "CFF ", out var maybeCff ) && maybeCff is CffTable_old cff ) {
			var evaluator = new CffTable_old.CharStringEvaluator(
				cff.GlobalSubrs,
				cff.PrivateDicts[0]?.Subrs
			);

			if ( cff.CharStrings[0] is CffTable_old.Index<CffTable_old.CharString> strs ) {
				var names = cff.Charsets[0]!.Glyphs;
				for ( int i = 0; i < strs.Count; i++ ) {
					var glyphName = i == 0 ? new CffTable_old.SID() : names[i - 1];
					var glyphCharString = strs.Data[i];
					var glyph = GetGlyph( new GlyphId( i ) );
					glyph.Names.Add( cff.GetString( glyphName ) );

					evaluator.Evaluate( glyphCharString, glyph.Outline );
				}
			}
		}
		else if ( tablesByTag.TryGetValue( "glyf", out var maybeGlyf ) && maybeGlyf is GlyphDataTable_old glyf ) {
			for ( int i = 0; i < glyf.Glyphs.Length; i++ ) {
				var glyphData = glyf.Glyphs[i];
				if ( glyphData == null )
					continue;

				var glyph = GetGlyph( new GlyphId( i ) );
				//if ( !glyph.AssignedRunes.Contains( new Rune( 'w' ) ) )
				//	continue;

				glyphData.CopyOutline( glyph.Outline );
				glyph.MinX = glyphData.MinX;
				glyph.MinY = glyphData.MinY;
				glyph.MaxX = glyphData.MaxX;
				glyph.MaxY = glyphData.MaxY;
			}
		}
		else {
			throw new InvalidDataException( "No glyph data is present in font" );
		}
		

		var hmtx = (HorizontalMetricsTable_old)tablesByTag["hmtx"]!;
		for ( int i = 0; i < hmtx.HMetrics.Length; i++ ) { // TODO remaining lsbs
			var glyph = GetGlyph( new GlyphId( i ) );
			var metric = hmtx.HMetrics[i];
			glyph.HorizontalAdvance = metric.AdvanceWidth;
			glyph.MinX = metric.LeftSideBearing;
		}
	}

	protected override void TryLoadGlyphFor ( Rune rune ) {
		
	}

	public static Rune Decode ( ushort charcode, int platform, int encoding ) {
		if ( platform == 0 && encoding == 3 ) { // unicode 2.0
			return new Rune( (char)charcode );
		}
		else if ( platform == 1 && encoding == 0 ) { // macintosh roman
			return MacintoshRomanEncoding.Decode( (byte)charcode );
		}
		else if ( platform == 3 && encoding == 1 ) { // unicode
			return new Rune( (char)charcode );
		}
		else {
			throw new Exception( "Unsupported encoding" );
		}
	}

	public static string Decode ( ReadOnlySpan<byte> data, int platform, int encoding ) {
		if ( platform == 0 && encoding == 3 ) {
			return Encoding.BigEndianUnicode.GetString( data );
		}
		else if ( platform == 1 && encoding == 0 ) {
			StringBuilder sb = new( data.Length );
			foreach ( var i in data ) {
				sb.Append( MacintoshRomanEncoding.Decode( i ).ToString() );
			}
			return sb.ToString();
		}
		else if ( platform == 3 && encoding == 1 ) {
			return Encoding.BigEndianUnicode.GetString( data );
		}
		else {
			throw new Exception( "Unsupported encoding" );
		}
	}

	public static OpenTypeFont_old FromStream ( Stream stream ) {
		using var reader = new EndianCorrectingBinaryReader( stream, isLitteEndian: false );
		return new( BinaryFileParser.Parse<OpenFontFile_old>( reader ) );
	}
}
