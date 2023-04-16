using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Adobe;

public static class OperandEncoding {
	public static bool IsDictOperand ( byte code ) {
		return code is ((>= 32) or 28 or 29) and not 255;
	}

	public static int GetDictOperandSize ( byte code ) {
		return code switch {
			>= 32 and <= 246 => 1,
			>= 246 and <= 250 => 2,
			>= 251 and <= 254 => 2,
			28 => 3,
			_ => 5 // 29
		};
	}

	public static double DecodeDictOperand ( BinaryArrayView<byte> data, ref int index ) {
		var b0 = data[index];
		if ( b0 is >= 32 and <= 246 ) {
			index += 1;
			return b0 - 139;
		}
		if ( b0 is >= 246 and <= 250 ) {
			var b1 = data[index + 1];
			index += 2;
			return ( b0 - 247 ) * 256 + b1 + 108;
		}	
		if ( b0 is >= 251 and <= 254 ) {
			var b1 = data[index + 1];
			index += 2;
			return -( b0 - 251 ) * 256 - b1 - 108;
		}
		if ( b0 is 28 ) {
			var b1 = data[index + 1];
			var b2 = data[index + 2];
			index += 3;
			return ( b1 << 8 ) | b2;
		}
		else { // b0 is 29
			var b1 = data[index + 1];
			var b2 = data[index + 2];
			var b3 = data[index + 3];
			var b4 = data[index + 4];
			index += 5;
			return ( b1 << 24 ) | ( b2 << 16 ) | ( b3 << 8 ) | ( b4 );
		}
	}
}
