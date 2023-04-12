namespace Vit.Framework.Text.Fonts.OpenType.Tables;

public class HeadTable : Table {
	public ushort MajorVersion;
	public ushort MinorVersion;
	public Fixed16_16 FontRevision;
	public uint ChecksumAdjustment;
	public uint MagicNumber;
	public ushort Flags;
	public ushort UnitsPerEm;

	public LongDateTime Created;
	public LongDateTime Modified;

	public short XMin;
	public short YMin;
	public short XMax;
	public short YMax;

	public ushort MacStyle;
	public ushort LowestRecPPEM;
	public short FontDirectionHint;
	public short IndexToLocFormat;
	public short GlyphDataFormat;
}
