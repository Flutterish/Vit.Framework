using Vit.Framework.Mathematics;

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
	public readonly GlyphId Id;
	public readonly HashSet<string> Names = new();
	public readonly HashSet<string> AssignedVectors = new();
	public readonly Outline<double> Outline = new();

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

	public Glyph ( GlyphId id ) {
		Id = id;
	}

	public override string ToString () {
		var name = Names.Count == 1 ? $"`{Names.Single()}` " : $"";
		return $"Glyph {Id.Value} {name}({AssignedVectors.Count} Rune{(AssignedVectors.Count == 1 ? "" : "s")}) `{string.Join("", AssignedVectors)}`";
	}
}