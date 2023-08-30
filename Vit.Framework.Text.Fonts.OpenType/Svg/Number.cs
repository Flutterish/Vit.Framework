namespace Vit.Framework.Text.Fonts.OpenType.Svg;

public static class Number {
	public static double Parse ( ByteString data ) {
		if ( !number( ref data, out var value ) )
			throw new InvalidDataException( "Could not parse number" );

		return value;
	}

	public static bool number ( ref ByteString data, out double value ) {
		var checkpoint = data;

		sign( ref data, out var signMultiplier );
		if ( floatingPointConstant( ref data, out value ) || integerConstant( ref data, out value ) ) {
			value *= signMultiplier;
			return true;
		}

		data = checkpoint;
		return false;
	}

	static bool integerConstant ( ref ByteString data, out double value ) {
		return digitSequence( ref data, out value );
	}

	static bool floatingPointConstant ( ref ByteString data, out double value ) {
		var checkpoint = data;

		if ( fractionalConstant( ref data, out value ) ) {
			if ( exponent( ref data, out var exp ) ) {
				value *= double.Pow( 10, exp );
			}

			return true;
		}

		data = checkpoint;
		if ( digitSequence( ref data, out value ) && exponent( ref data, out var exp2 ) ) {
			value *= double.Pow( 10, exp2 );
			return true;
		}

		value = 0;
		data = checkpoint;
		return false;
	}

	static bool fractionalConstant ( ref ByteString data, out double value ) {
		var checkpoint = data;

		if ( digitSequence( ref data, out var integer ) ) {
			if ( !single( ref data, '.' ) ) {
				data = checkpoint;
				value = 0;
				return false;
			}

			if ( digitSequence( ref data, out var fractional, fractional: true ) ) {
				value = integer + fractional;
				return true;
			}

			value = integer;
			return true;
		}

		if ( !single( ref data, '.' ) ) {
			value = 0;
			return false;
		}

		if ( digitSequence( ref data, out value, fractional: true ) )
			return true;
		else {
			data = checkpoint;
			return false;
		}
	}

	static bool exponent ( ref ByteString data, out double value ) {
		var checkpoint = data;
		value = 1;
		if ( data.Length == 0 )
			return false;

		var c = data[0];
		if ( c is not ('e' or 'E') )
			return false;

		data = data.Slice( 1 );
		if ( sign( ref data, out var multiplier ) )
			value *= multiplier;

		if ( !digitSequence( ref data, out var digits ) ) {
			data = checkpoint;
			return false;
		}

		value *= digits;
		return true;
	}

	static bool sign ( ref ByteString data, out double multiplier ) {
		multiplier = 1;
		if ( data.Length == 0 )
			return false;

		var c = data.Bytes[0];

		if ( c == '+' ) {
			data = data.Slice( 1 );
			return true;
		}
		else if ( c == '-' ) {
			data = data.Slice( 1 );
			multiplier = -1;
			return true;
		}

		return false;
	}

	static bool digitSequence ( ref ByteString data, out double value, bool fractional = false ) {
		if ( !digit( ref data, out value ) )
			return false;

		int count = 1;
		while ( digit( ref data, out var nextDigit ) ) {
			value *= 10;
			value += nextDigit;
			count++;
		}

		if ( fractional )
			value /= double.Pow( 10, count );

		return true;
	}

	static bool digit ( ref ByteString data, out double digit ) {
		if ( data.Length == 0 ) {
			digit = 0;
			return false;
		}

		var c = data.Bytes[0];
		if ( c is >= 48 and <= 57 ) {
			data = data.Slice( 1 );
			digit = c - 48;
			return true;
		}
		digit = 0;
		return false;
	}

	static bool single ( ref ByteString data, char match ) {
		if ( data.Length == 0 )
			return false;

		if ( data[0] == match ) {
			data = data.Slice( 1 );
			return true;
		}
		return false;
	}
}
