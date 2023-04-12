using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

[ParserDependency]
public class HorizontalHeaderTable : Table {
	public ushort MajorVersion;
	public ushort MinorVersion;

	public FontWord Ascender;
	public FontWord Descender;
	public FontWord LineGap;
	public UFontWord AdvanceWidthMax;
	public FontWord MinLeftSizeBearing;
	public FontWord MinRightSizeBearing;
	public FontWord XMaxExtent;

	public short CaretSlopeRise;
	public short CaretSlopeRun;
	public short CaretOffset;

	public short reserved_0;
	public short reserved_1;
	public short reserved_2;
	public short reserved_3;

	public short MetricDataFormat;
	public ushort NumberOfHMetrics;
}
