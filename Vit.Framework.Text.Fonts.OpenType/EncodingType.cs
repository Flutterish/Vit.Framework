using System.Text;
using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType;

public enum EncodingType {
	Unicode,
	MacintoshRoman
}

public static class EncodingTypeExtensions {
	public static Rune Decode ( this EncodingType type, int charcode ) {
		if ( type == EncodingType.MacintoshRoman )
			return charcode > 255 ? new Rune('\0') : MacintoshRomanEncoding.Decode( (byte)charcode );

		return new Rune( charcode );
	}

	public static string Decode ( this EncodingType type, BinaryArrayView<byte> data ) {
		using var bytes = data.GetRented();
		return type.Decode( bytes.AsSpan() );
	}

	public static string Decode ( this EncodingType type, ReadOnlySpan<byte> data ) {
		if ( type == EncodingType.MacintoshRoman ) {
			StringBuilder sb = new( data.Length );
			foreach ( var i in data ) {
				sb.Append( MacintoshRomanEncoding.Decode( i ).ToString() );
			}
			return sb.ToString();
		}

		return Encoding.BigEndianUnicode.GetString( data );
	}

	public static EncodingType GetEncodingType ( int platform, int encoding ) {
		if ( platform == 0 && encoding == 3 ) {
			return EncodingType.Unicode;
		}
		else if ( platform == 1 && encoding == 0 ) {
			return EncodingType.MacintoshRoman;
		}
		else if ( platform == 3 && encoding == 1 ) {
			return EncodingType.Unicode;
		}
		else {
			throw new Exception( "Unsupported encoding" );
		}
	}
}