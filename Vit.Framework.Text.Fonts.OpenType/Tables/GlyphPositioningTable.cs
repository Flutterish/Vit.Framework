using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

[TypeSelector(nameof(selectType))]
public class GlyphPositioningTable : Table {
	public ushort MajorVersion;
	public ushort MinorVersion;
	public Offset16 ScriptListOffset;
	public Offset16 FeatureListOffset;
	public Offset16 LookupListOffset;

	[DataOffset( nameof( ScriptListOffset ) )]
	public ScriptListTable ScriptList = null!;

	[DataOffset( nameof( FeatureListOffset ) )]
	public FeatureListTable FeatureList = null!;

	[DataOffset( nameof( LookupListOffset ) )]
	public LookupListTable LookupList = null!;

	static Type? selectType ( ushort majorVersion, ushort minorVersion ) {
		if ( majorVersion == 1 && minorVersion == 1 )
			return typeof( GlyphPositioningTable1_1 );
		return null;
	}

	public class ScriptListTable {

	}

	public class FeatureListTable {

	}

	public class LookupListTable {

	}
}

public class GlyphPositioningTable1_1 : GlyphPositioningTable {
	public Offset32 FeatureVariantOffset;
}
