using Vit.Framework.Parsing;
using Vit.Framework.Parsing.Binary;
using Vit.Framework.Text.Fonts.OpenType.Tables;

namespace Vit.Framework.Text.Fonts.OpenType;

public class OpenTypeFont : Font {
	public OpenFontFile Data;
	Dictionary<Tag, Table> TablesByTag = new();

	public OpenTypeFont ( OpenFontFile data ) {
		Data = data;
		foreach ( var i in data.TableRecords ) {
			TablesByTag[i.TableTag] = i.Table;
		}
	}

	public static OpenTypeFont FromStream ( Stream stream ) {
		using var reader = new EndianCorrectingBinaryReader( stream, isLitteEndian: false );
		return new( BinaryFileParser.Parse<OpenFontFile>( reader ) );
	}
}
