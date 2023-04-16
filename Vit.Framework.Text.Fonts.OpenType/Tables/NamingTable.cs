using System.Text;
using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

[TypeSelector( nameof( selectType ) )]
public class NamingTable : Table {
	public ushort Version;
	public ushort Count;
	[Cache]
	public Offset16 StorageOffset;
	[Size( nameof( Count ) )]
	public BinaryArrayView<NameRecord> NameRecords;

	static Type? selectType ( ushort version ) {
		return version switch {
			1 => typeof( NamingTableVersion1 ),
			_ => typeof( NamingTable )
		};
	}

	public struct NameRecord {
		public ushort PlatformId;
		public ushort EncodingId;
		public ushort LanguageId;
		public ushort NameId;
		public ushort Length;
		public Offset16 StringOffset;

		[DataOffset( nameof( offset ) )]
		[Size( nameof( Length ) )]
		public BinaryArrayView<byte> StringData;

		static int offset ( Offset16 stringOffset, [Resolve] Offset16 storageOffset ) {
			return stringOffset.Value + storageOffset.Value;
		}

		public override string ToString () {
			return EncodingTypeExtensions.GetEncodingType( PlatformId, EncodingId ).Decode( StringData );
		}
	}
}

public class NamingTableVersion1 : NamingTable {
	public ushort LangTagCount;
	[Size( nameof( LangTagCount ) )]
	public BinaryArrayView<LangTagRecord> LangTagRecords;

	public struct LangTagRecord {
		public ushort Length;
		public Offset16 LangTagOffset;

		[DataOffset( nameof( offset ) )]
		[Size( nameof( Length ) )]
		public BinaryArrayView<byte> StringData;

		static int offset ( Offset16 langTagOffset, [Resolve] Offset16 storageOffset ) {
			return langTagOffset.Value + storageOffset.Value;
		}

		public override string ToString () {
			using var array = StringData.GetRented();
			return Encoding.UTF8.GetString( array );
		}
	}
}