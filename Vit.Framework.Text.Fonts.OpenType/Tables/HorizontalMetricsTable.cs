using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

public class HorizontalMetricsTable : Table {
	[Size( nameof( getSize ) )]
	public BinaryArrayView<LongHorizontalMetric> HorizontalMetrics;
	[Size( nameof( getSize2 ) )]
	public BinaryArrayView<short> LeftSideBearings;

	static int getSize ( [Resolve] HorizontalHeaderTable header ) {
		return header.NumberOfHMetrics;
	}

	static int getSize2 ( [Resolve] HorizontalHeaderTable header, [Resolve] MaximumProfileTable profile ) {
		return profile.GlyphCount - header.NumberOfHMetrics;
	}

	public struct LongHorizontalMetric {
		public ushort AdvanceWidth;
		public short LeftSideBearing;
	}
}