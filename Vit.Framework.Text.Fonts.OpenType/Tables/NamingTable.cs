using System.Text;
using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

[TypeSelector(nameof(selectType))]
public class NamingTable : Table {
	public ushort Version;
	public ushort Count;
	[ParserDependency]
	public Offset16 StorageOffset;
	[Size(nameof(Count))]
	public NameRecord[] NameRecords = null!;

	static Type? selectType ( ushort version ) {
		return version switch {
			1 => typeof( NamingTableVersion1 ),
			_ => null
		};
	}

	public struct NameRecord {
		public ushort PlatformId;
		public ushort EncodingId;
		public ushort LanguageId;
		public ushort NameId;
		public ushort Length;
		public Offset16 StringOffset;

		[DataOffset(nameof( offset ) )]
		[Size(nameof(Length))]
		public byte[] StringData;

		static int offset ( Offset16 stringOffset, Offset16 storageOffset ) {
			return stringOffset.Value + storageOffset.Value;
		}

		public override string ToString () {
			return (PlatformId == 3 ? Encoding.BigEndianUnicode : Encoding.UTF8).GetString( StringData );
		}
	}
}

public class NamingTableVersion1 : NamingTable {
	public ushort LangTagCount;
	[Size(nameof(LangTagCount))]
	public LangTagRecord[] LangTagRecords = null!;

	public struct LangTagRecord {
		public ushort Length;
		public Offset16 LangTagOffset;

		[DataOffset( nameof( offset ) )]
		[Size( nameof( Length ) )]
		public byte[] StringData;

		static int offset ( Offset16 langTagOffset, Offset16 storageOffset ) {
			return langTagOffset.Value + storageOffset.Value;
		}

		public override string ToString () {
			return Encoding.UTF8.GetString( StringData );
		}
	}
}