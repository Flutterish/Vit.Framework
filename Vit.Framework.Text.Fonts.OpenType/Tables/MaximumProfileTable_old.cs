﻿using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

[ParserDependency]
[TypeSelector(nameof(selectType))]
public class MaximumProfileTable_old : Table {
	public Version16_16 Version;
	public ushort GlyphCount;

	static Type? selectType ( Version16_16 version ) {
		return version.Major == 1 && version.Minor == 0 ? typeof( MaximumProfileTable_oldVersion1 ) : typeof( MaximumProfileTable_old );
	}
}

public class MaximumProfileTable_oldVersion1 : MaximumProfileTable_old {
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