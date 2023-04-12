using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

[ParsingDependsOn(typeof(HorizontalHeaderTable), typeof(MaximumProfileTable))]
public class HorizontalMetricsTable : Table {
	[Size(nameof(getSize))]
	public LongHorMetric[] HMetrics = null!;
	[Size(nameof(getSize2))]
	public short[] leftSideBearings = null!;

	static int getSize ( HorizontalHeaderTable header ) {
		return header.NumberOfHMetrics;
	}

	static int getSize2 ( HorizontalHeaderTable header, MaximumProfileTable profile ) {
		return profile.GlyphCount - header.NumberOfHMetrics;
	}

	public struct LongHorMetric {
		public ushort AdvanceWidth;
		public short LeftSideBearing;
	}
}
