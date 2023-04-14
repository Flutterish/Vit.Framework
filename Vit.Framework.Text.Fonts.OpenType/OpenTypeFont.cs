using Vit.Framework.Parsing;
using Vit.Framework.Parsing.Binary;
using Vit.Framework.Text.Fonts.OpenType.Tables;

namespace Vit.Framework.Text.Fonts.OpenType;

public class OpenTypeFont : Font {
	public OpenFontFile Data;

	public OpenTypeFont ( OpenFontFile data ) {
		Data = data;

		var tablesByTag = new Dictionary<Tag, Table?>();
		foreach ( var i in data.TableRecords ) {
			tablesByTag[i.TableTag] = i.Table;
		}

		var cmap = (CmapTable)tablesByTag["cmap"]!;
		foreach ( var record in cmap.EncodingRecords ) {
			var subtable = record.Subtable;
			foreach ( var (rune, glyph) in subtable.Glyphs ) {
				AddGlyphMapping( rune, glyph );
			}
		}

		var name = (NamingTable)tablesByTag["name"]!;
		foreach ( var i in name.NameRecords ) {
			if ( i.NameId == 4 ) {
				Name = i.ToString();
				break;
			}
		}

		var cff = (CffTable)tablesByTag["CFF "]!;
		var evaluator = new CffTable.CharStringEvaluator(
			cff.GlobalSubrs,
			cff.PrivateDicts[0]?.Subrs
		);

		if ( cff.CharStrings[0] is CffTable.Index<CffTable.CharString> strs ) {
			var names = cff.Charsets[0]!.Glyphs;
			for ( int i = 0; i < strs.Count; i++ ) {
				var glyphName = i == 0 ? new CffTable.SID() : names[i - 1];
				var glyphCharString = strs.Data[i];

				evaluator.Evaluate( glyphCharString );
			}
		}
	}

	public static OpenTypeFont FromStream ( Stream stream ) {
		using var reader = new EndianCorrectingBinaryReader( stream, isLitteEndian: false );
		return new( BinaryFileParser.Parse<OpenFontFile>( reader ) );
	}
}
