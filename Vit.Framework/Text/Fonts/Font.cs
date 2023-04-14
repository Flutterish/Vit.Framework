using System.Text;

namespace Vit.Framework.Text.Fonts;

public class Font {
	protected Dictionary<GlyphId, Glyph> GlyphsById = new();
	protected Dictionary<Rune, HashSet<Glyph>> GlyphsByRune = new();

	public string Name { get; protected set; } = string.Empty;

	protected void AddGlyphMapping ( Rune rune, GlyphId id ) {
		if ( !GlyphsById.TryGetValue( id, out var glyph ) )
			GlyphsById.Add( id, glyph = new( id ) );

		if ( !GlyphsByRune.TryGetValue( rune, out var set ) )
			GlyphsByRune.Add( rune, set = new() );

		if ( set.Add( glyph ) ) {
			glyph.AssignedRunes.Add( rune );
		}
	}
}
