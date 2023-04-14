using System.Text;

namespace Vit.Framework.Text.Fonts;

public struct GlyphId {
	public ulong Id;

	public GlyphId ( ulong id ) {
		Id = id;
	}

	public static implicit operator GlyphId ( ulong id ) => new( id );
	public static implicit operator GlyphId ( uint id ) => new( id );

	public override string ToString () {
		return $"GlyphId {Id}";
	}
}

public class Glyph {
	public readonly GlyphId Id;
	public readonly List<Rune> AssignedRunes = new();

	public Glyph ( GlyphId id ) {
		Id = id;
	}

	public override string ToString () {
		return $"Glyph {Id.Id} ({AssignedRunes.Count} Rune{(AssignedRunes.Count == 1 ? "" : "s")}) `{string.Join("", AssignedRunes)}`";
	}
}