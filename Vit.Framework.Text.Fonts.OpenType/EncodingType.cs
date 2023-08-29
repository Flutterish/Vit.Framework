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
		return (platform, encoding) switch {
			(0, _) => EncodingType.Unicode,
			(1, 0) => EncodingType.MacintoshRoman,
			(3, 1) => EncodingType.Unicode,
			(3, 10) => EncodingType.Unicode,
			_ => throw new Exception( "Unsupported encoding" )
		};
	}
}