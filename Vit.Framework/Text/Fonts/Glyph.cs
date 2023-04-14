using System.Text;

namespace Vit.Framework.Text.Fonts;

public struct GlyphId {
	public ulong Id;

	public GlyphId ( ulong id ) {
		Id = id;
	}
	public GlyphId ( int id ) {
		Id = (ulong)id;
	}

	public static implicit operator GlyphId ( ulong id ) => new( id );
	public static implicit operator GlyphId ( uint id ) => new( id );

	public override string ToString () {
		return $"GlyphId {Id}";
	}
}

public class Glyph {
	public readonly GlyphId Id;
	public readonly HashSet<string> Names = new();
	public readonly HashSet<Rune> AssignedRunes = new();
	public readonly Outline<double> Outline = new();

	public double MinX;
	public double MaxX;
	public double MinY;
	public double MaxY;

	public double HorizontalAdvance;
	public double VerticalAdvance;

	public double BearingX => MinX;
	public double BearingY => MaxY;
	public double Width => MaxX - MinX;
	public double Height => MaxY - MinY;

	public Glyph ( GlyphId id ) {
		Id = id;
	}

	public override string ToString () {
		var name = Names.Count == 1 ? $"`{Names.Single()}` " : $"";
		return $"Glyph {Id.Id} {name}({AssignedRunes.Count} Rune{(AssignedRunes.Count == 1 ? "" : "s")}) `{string.Join("", AssignedRunes)}`";
	}
}