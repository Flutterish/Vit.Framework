using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

[ParsingDependsOn(typeof(HorizontalHeaderTable_old), typeof(MaximumProfileTable_old))]
public class HorizontalMetricsTable_old : Table {
	[Size(nameof(getSize))]
	public LongHorMetric[] HMetrics = null!;
	[Size(nameof(getSize2))]
	public short[] leftSideBearings = null!;

	static int getSize ( HorizontalHeaderTable_old header ) {
		return header.NumberOfHMetrics;
	}

	static int getSize2 ( HorizontalHeaderTable_old header, MaximumProfileTable_old profile ) {
		return profile.GlyphCount - header.NumberOfHMetrics;
	}

	public struct LongHorMetric {
		public ushort AdvanceWidth;
		public short LeftSideBearing;
	}
}
