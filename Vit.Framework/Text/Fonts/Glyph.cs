using Vit.Framework.Mathematics;
using Vit.Framework.Text.Outlines;

namespace Vit.Framework.Text.Fonts;

public struct GlyphId {
	public ulong Value;

	public GlyphId ( ulong id ) {
		Value = id;
	}
	public GlyphId ( int id ) {
		Value = (ulong)id;
	}

	public static implicit operator GlyphId ( ulong id ) => new( id );
	public static implicit operator GlyphId ( uint id ) => new( id );

	public override string ToString () {
		return $"GlyphId {Value}";
	}
}

public class Glyph {
	public readonly Font Font;
	public readonly GlyphId Id;
	public readonly HashSet<string> Names = new();
	public readonly HashSet<string> AssignedVectors = new();
	public readonly SplineOutline<double> Outline = new(); // TODO 1. an outline should be loaded on demand and cached only when requested. 2. the outline type should be an abstract class, and the type should be given when requested

	public double MinX;
	public double MaxX;
	public double MinY;
	public double MaxY;

	public double HorizontalAdvance;
	public double VerticalAdvance;

	public double LeftBearing => MinX;
	public double RightBearing => HorizontalAdvance - MaxX;
	public double TopBearing => MaxY;
	public double BottomBearing => VerticalAdvance - MinY;
	public double Width => MaxX - MinX;
	public double Height => MaxY - MinY;

	AxisAlignedBox2<double>? calculatedBoundingBox;
	public AxisAlignedBox2<double> CalculatedBoundingBox {
		get => calculatedBoundingBox ??= Outline.CalculateBoundingBox();
		set => calculatedBoundingBox = null;
	}

	public AxisAlignedBox2<double> DefinedBoundingBox => new AxisAlignedBox2<double> {
		MinX = MinX,
		MinY = MinY,
		MaxX = MaxX,
		MaxY = MaxY
	};

	public Glyph ( GlyphId id, Font font ) {
		Id = id;
		Font = font;
	}

	public override string ToString () {
		var name = Names.Count == 1 ? $"`{Names.Single()}` " : Id.Value == 0 ? "`Null` " : "";
		var runes = Id.Value == 0 ? "" : $"`{string.Join( "", AssignedVectors.Select( x => x.Length == 1 ? char.IsControl( x[0] ) ? $"U+{(uint)x[0]:X4}" : x : x ) )}` ";
		return $"Glyph {Id.Value} {name}({AssignedVectors.Count} Grapheme{(AssignedVectors.Count == 1 ? "" : "s")}) {runes}in {Font.Name}";
	}
}