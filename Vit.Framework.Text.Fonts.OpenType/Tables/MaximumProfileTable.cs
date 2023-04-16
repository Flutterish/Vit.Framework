using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

[TypeSelector( nameof( selectType ) )]
public class MaximumProfileTable : Table {
	public Version16_16 Version;
	public ushort GlyphCount;

	static Type? selectType ( Version16_16 version ) {
		return version.Major == 1 && version.Minor == 0 ? typeof( MaximumProfileTableVersion1 ) : typeof( MaximumProfileTable );
	}
}

public class MaximumProfileTableVersion1 : MaximumProfileTable {
	public ushort MaxPoints;
	public ushort MaxContours;
	public ushort MaxCompositePoints;
	public ushort MaxCompositeContours;
	public ushort MaxZones;
	public ushort MaxTwilightPoints;
	public ushort MaxStorage;
	public ushort MaxFunctionDefs;
	public ushort MaxInstructionDefs;
	public ushort MaxStackElements;
	public ushort MaxSizeOfInstructions;
	public ushort MaxCompositeElements;
	public ushort MaxComponentDepth;
}