using System.Text;

namespace Vit.Framework.Text.Fonts;

public class FontCollection {
	List<Font> fonts = new();
	public FontCollection ( IEnumerable<Font> fonts ) {
		this.fonts.AddRange( fonts );
	}

	public Glyph GetGlyph ( ReadOnlySpan<char> graphemeCluster ) {
		Span<byte> bytes = stackalloc byte[Encoding.UTF32.GetByteCount( graphemeCluster )];
		Encoding.UTF32.GetBytes( graphemeCluster, bytes );

		return GetGlyph( new UnicodeExtendedGraphemeCluster( bytes ) );
	}

	CodepointPrefixTree<Glyph> resolvedGlyphs = new();
	public Glyph GetGlyph ( UnicodeExtendedGraphemeCluster cluster ) {
		if ( resolvedGlyphs.TryGetValue( cluster, out var cached ) )
			return cached;

		foreach ( var font in fonts ) {
			var glyph = font.GetGlyph( cluster );
			if ( glyph.Id.Value != 0 ) {
				resolvedGlyphs.Add( cluster, glyph );
				return glyph;
			}
		}

		Span<byte> nullVector = stackalloc byte[4];
		nullVector.Fill( 0 );
		var nullGlyph = fonts[0].GetGlyph( new UnicodeExtendedGraphemeCluster( nullVector ) );
		resolvedGlyphs.Add( cluster, nullGlyph );
		return nullGlyph;
	}

	public double PrimaryUnitsPerEm => fonts[0].UnitsPerEm;
}
