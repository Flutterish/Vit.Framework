using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Adobe;

public static class OperatorEncoding {
	public static bool IsDictOperator ( byte code ) {
		return code is (< 32) and not (28 or 29);
	}

	public static int GetDictOperatorSize ( byte code ) {
		return code == 12 ? 2 : 1;
	}

	public static ushort DecodeDictOperator ( BinaryArrayView<byte> data, ref int index ) {
		var b0 = data[index];
		if ( b0 == 12 ) {
			var b1 = data[index + 1];
			index += 2;
			return (ushort)( 12 << 8 | b1 );
		}

		index++;
		return b0;
	}
}
