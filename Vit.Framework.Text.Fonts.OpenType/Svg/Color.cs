﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics;

namespace Vit.Framework.Text.Fonts.OpenType.Svg;

public static class Color {
	public static ColorSRgb<byte>? Parse ( ByteString data ) {
		var str = data.ToString();

		if ( str.Length == 0 )
			throw new InvalidDataException();

		if ( str[0] != '#' )
			throw new InvalidDataException();

		if ( str.Length == 4 ) {
			return new() {
				R = byte.Parse( $"{str[1]}{str[1]}", System.Globalization.NumberStyles.HexNumber ),
				G = byte.Parse( $"{str[2]}{str[2]}", System.Globalization.NumberStyles.HexNumber ),
				B = byte.Parse( $"{str[3]}{str[3]}", System.Globalization.NumberStyles.HexNumber )
			};
		}
		else if ( str.Length == 7 ) {
			return new() {
				R = byte.Parse( $"{str[1]}{str[2]}", System.Globalization.NumberStyles.HexNumber ),
				G = byte.Parse( $"{str[3]}{str[4]}", System.Globalization.NumberStyles.HexNumber ),
				B = byte.Parse( $"{str[5]}{str[6]}", System.Globalization.NumberStyles.HexNumber )
			};
		}
		else {
			throw new NotImplementedException();
		}
	}
}
