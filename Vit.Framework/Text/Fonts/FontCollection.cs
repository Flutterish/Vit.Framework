using System.Text;

namespace Vit.Framework.Text.Fonts;

public class FontCollection {
	List<Font> fonts = new();
	public FontCollection ( IEnumerable<Font> fonts ) {
		this.fonts.AddRange( fonts );
	}

	public unsafe Glyph GetGlyph ( ReadOnlySpan<char> graphemeCluster ) {
		var length = Encoding.UTF32.GetByteCount( graphemeCluster );
		byte* bytes = stackalloc byte[length];
		Encoding.UTF32.GetBytes( graphemeCluster, new Span<byte>( bytes, length ) );

		return GetGlyph( new UnicodeExtendedGraphemeCluster( bytes, length ) );
	}

	CodepointPrefixTree<Glyph> resolvedGlyphs = new();
	public unsafe Glyph GetGlyph ( UnicodeExtendedGraphemeCluster cluster ) {
		if ( resolvedGlyphs.TryGetValue( cluster, out var cached ) )
			return cached;

		foreach ( var font in fonts ) {
			var glyph = font.GetGlyph( cluster );
			if ( glyph.Id.Value != 0 ) {
				resolvedGlyphs.Add( cluster, glyph );
				return glyph;
			}
		}

		byte* nullVector = stackalloc byte[4];
		nullVector[0] = 0;
		nullVector[1] = 0;
		nullVector[2] = 0;
		nullVector[3] = 0;
		var nullGlyph = fonts[0].GetGlyph( new UnicodeExtendedGraphemeCluster( nullVector, 4 ) );
		resolvedGlyphs.Add( cluster, nullGlyph );
		return nullGlyph;
	}

	public double PrimaryUnitsPerEm => fonts[0].UnitsPerEm;
}
