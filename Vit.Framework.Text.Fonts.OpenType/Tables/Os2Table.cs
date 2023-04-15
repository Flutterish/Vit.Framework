using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

[TypeSelector(nameof(selectType))]
public class Os2Table : Table {
	public ushort Version;

	static Type? selectType ( ushort version ) {
		return version switch {
			3 => typeof(Os2TableVersion3),
			_ => typeof( Os2Table )
		};
	}
}

public class Os2TableVersion3 : Os2Table {
	public short XAvgCharWidth;
	public ushort WeightClass;
	public ushort WidthClass;
	public ushort FsType;
	public short YSubscriptXSize;
	public short YSubscriptYSize;
	public short YSubscriptXOffset;
	public short YSubscriptYOffset;
	public short YSuperscriptXSize;
	public short YSuperscriptYSize;
	public short YSuperscriptXOffset;
	public short YSuperscriptYOffset;
	public short YStrikeoutSize;
	public short YStrikeoutPosition;
	public short FamilyClass;
	[Size( 10 )]
	public byte[] Panose = null!;
	public uint UnicodeRange1;
	public uint UnicodeRange2;
	public uint UnicodeRange3;
	public uint UnicodeRange4;
	public Tag AchVendID;
	public ushort FsSelection;
	public ushort FirstCharIndex;
	public ushort LastCharIndex;
	public short TypoAscender;
	public short TypoDescender;
	public short TypoLineGap;
	public ushort WinAscent;
	public ushort WinDescent;
	public uint CodePageRange1;
	public uint CodePageRange2;
	public short XHeight;
	public short CapHeight;
	public ushort DefaultChar;
	public ushort BreakChar;
	public ushort MaxContext;
}