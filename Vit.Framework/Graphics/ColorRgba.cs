using System.Numerics;

namespace Vit.Framework.Graphics;

public struct ColorRgba<T> : IEqualityOperators<ColorRgba<T>, ColorRgba<T>, bool> where T : INumber<T> {
	public T R;
	public T G;
	public T B;
	public T A;

	public ColorRgba ( T r, T g, T b, T a ) {
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public static ColorRgba<T> operator / ( ColorRgba<T> color, T scalar ) {
		return new() {
			R = color.R / scalar,
			G = color.G / scalar,
			B = color.B / scalar,
			A = color.A
		};
	}

	public static bool operator == ( ColorRgba<T> left, ColorRgba<T> right ) {
		return left.R == right.R
			&& left.G == right.G
			&& left.B == right.B
			&& left.A == right.A;
	}

	public static bool operator != ( ColorRgba<T> left, ColorRgba<T> right ) {
		return left.R != right.R
			|| left.G != right.G
			|| left.B != right.B
			|| left.A != right.A;
	}

	public override string ToString () {
		if ( typeof(T) == typeof(byte) ) {
			var _255 = T.CreateChecked( 255 );
			return $"#{R:X2}{G:X2}{B:X2}{( A == _255 ? "" : $"{A:X2}" )}";
		}
		else if ( typeof(T).IsAssignableTo(typeof(IFloatingPoint<>)) ) {
			var _255 = T.CreateChecked( 255 );
			var (r, g, b, a) = (byte.CreateTruncating(R * _255),byte.CreateTruncating(G * _255), byte.CreateTruncating(B * _255), byte.CreateTruncating(A * _255));
			return $"#{r:X2}{g:X2}{b:X2}{( a == 255 ? "" : $"{a:X2}" )}";
		}
		else {
			return $"({R}, {G}, {B}, {A})";
		}
	}
}

public static class ColorRgba {
	public static readonly ColorRgba<float> MediumVioletRed = new ColorRgba<float>( 199, 21, 133, 255 ) / 255;
	public static readonly ColorRgba<float> DeepPink = new ColorRgba<float>( 255, 20, 147, 255 ) / 255;
	public static readonly ColorRgba<float> PaleVioletRed = new ColorRgba<float>( 219, 112, 147, 255 ) / 255;
	public static readonly ColorRgba<float> HotPink = new ColorRgba<float>( 255, 105, 180, 255 ) / 255;
	public static readonly ColorRgba<float> LightPink = new ColorRgba<float>( 255, 182, 193, 255 ) / 255;
	public static readonly ColorRgba<float> Pink = new ColorRgba<float>( 255, 192, 203, 255 ) / 255;
	public static readonly ColorRgba<float> DarkRed = new ColorRgba<float>( 139, 0, 0, 255 ) / 255;
	public static readonly ColorRgba<float> Red = new ColorRgba<float>( 255, 0, 0, 255 ) / 255;
	public static readonly ColorRgba<float> Firebrick = new ColorRgba<float>( 178, 34, 34, 255 ) / 255;
	public static readonly ColorRgba<float> Crimson = new ColorRgba<float>( 220, 20, 60, 255 ) / 255;
	public static readonly ColorRgba<float> IndianRed = new ColorRgba<float>( 205, 92, 92, 255 ) / 255;
	public static readonly ColorRgba<float> LightCoral = new ColorRgba<float>( 240, 128, 128, 255 ) / 255;
	public static readonly ColorRgba<float> Salmon = new ColorRgba<float>( 250, 128, 114, 255 ) / 255;
	public static readonly ColorRgba<float> DarkSalmon = new ColorRgba<float>( 233, 150, 122, 255 ) / 255;
	public static readonly ColorRgba<float> LightSalmon = new ColorRgba<float>( 255, 160, 122, 255 ) / 255;
	public static readonly ColorRgba<float> OrangeRed = new ColorRgba<float>( 255, 69, 0, 255 ) / 255;
	public static readonly ColorRgba<float> Tomato = new ColorRgba<float>( 255, 99, 71, 255 ) / 255;
	public static readonly ColorRgba<float> DarkOrange = new ColorRgba<float>( 255, 140, 0, 255 ) / 255;
	public static readonly ColorRgba<float> Coral = new ColorRgba<float>( 255, 127, 80, 255 ) / 255;
	public static readonly ColorRgba<float> Orange = new ColorRgba<float>( 255, 165, 0, 255 ) / 255;
	public static readonly ColorRgba<float> DarkKhaki = new ColorRgba<float>( 189, 183, 107, 255 ) / 255;
	public static readonly ColorRgba<float> Gold = new ColorRgba<float>( 255, 215, 0, 255 ) / 255;
	public static readonly ColorRgba<float> Khaki = new ColorRgba<float>( 240, 230, 140, 255 ) / 255;
	public static readonly ColorRgba<float> PeachPuff = new ColorRgba<float>( 255, 218, 185, 255 ) / 255;
	public static readonly ColorRgba<float> Yellow = new ColorRgba<float>( 255, 255, 0, 255 ) / 255;
	public static readonly ColorRgba<float> PaleGoldenrod = new ColorRgba<float>( 238, 232, 170, 255 ) / 255;
	public static readonly ColorRgba<float> Moccasin = new ColorRgba<float>( 255, 228, 181, 255 ) / 255;
	public static readonly ColorRgba<float> PapayaWhip = new ColorRgba<float>( 255, 239, 213, 255 ) / 255;
	public static readonly ColorRgba<float> LightGoldenrodYellow = new ColorRgba<float>( 250, 250, 210, 255 ) / 255;
	public static readonly ColorRgba<float> LemonChiffon = new ColorRgba<float>( 255, 250, 205, 255 ) / 255;
	public static readonly ColorRgba<float> LightYellow = new ColorRgba<float>( 255, 255, 224, 255 ) / 255;
	public static readonly ColorRgba<float> Maroon = new ColorRgba<float>( 128, 0, 0, 255 ) / 255;
	public static readonly ColorRgba<float> Brown = new ColorRgba<float>( 165, 42, 42, 255 ) / 255;
	public static readonly ColorRgba<float> SaddleBrown = new ColorRgba<float>( 139, 69, 19, 255 ) / 255;
	public static readonly ColorRgba<float> Sienna = new ColorRgba<float>( 160, 82, 45, 255 ) / 255;
	public static readonly ColorRgba<float> Chocolate = new ColorRgba<float>( 210, 105, 30, 255 ) / 255;
	public static readonly ColorRgba<float> DarkGoldenrod = new ColorRgba<float>( 184, 134, 11, 255 ) / 255;
	public static readonly ColorRgba<float> Peru = new ColorRgba<float>( 205, 133, 63, 255 ) / 255;
	public static readonly ColorRgba<float> RosyBrown = new ColorRgba<float>( 188, 143, 143, 255 ) / 255;
	public static readonly ColorRgba<float> Goldenrod = new ColorRgba<float>( 218, 165, 32, 255 ) / 255;
	public static readonly ColorRgba<float> SandyBrown = new ColorRgba<float>( 244, 164, 96, 255 ) / 255;
	public static readonly ColorRgba<float> Tan = new ColorRgba<float>( 210, 180, 140, 255 ) / 255;
	public static readonly ColorRgba<float> Burlywood = new ColorRgba<float>( 222, 184, 135, 255 ) / 255;
	public static readonly ColorRgba<float> Wheat = new ColorRgba<float>( 245, 222, 179, 255 ) / 255;
	public static readonly ColorRgba<float> NavajoWhite = new ColorRgba<float>( 255, 222, 173, 255 ) / 255;
	public static readonly ColorRgba<float> Bisque = new ColorRgba<float>( 255, 228, 196, 255 ) / 255;
	public static readonly ColorRgba<float> BlanchedAlmond = new ColorRgba<float>( 255, 235, 205, 255 ) / 255;
	public static readonly ColorRgba<float> Cornsilk = new ColorRgba<float>( 255, 248, 220, 255 ) / 255;
	public static readonly ColorRgba<float> Indigo = new ColorRgba<float>( 75, 0, 130, 255 ) / 255;
	public static readonly ColorRgba<float> Purple = new ColorRgba<float>( 128, 0, 128, 255 ) / 255;
	public static readonly ColorRgba<float> DarkMagenta = new ColorRgba<float>( 139, 0, 139, 255 ) / 255;
	public static readonly ColorRgba<float> DarkViolet = new ColorRgba<float>( 148, 0, 211, 255 ) / 255;
	public static readonly ColorRgba<float> DarkSlateBlue = new ColorRgba<float>( 72, 61, 139, 255 ) / 255;
	public static readonly ColorRgba<float> BlueViolet = new ColorRgba<float>( 138, 43, 226, 255 ) / 255;
	public static readonly ColorRgba<float> DarkOrchid = new ColorRgba<float>( 153, 50, 204, 255 ) / 255;
	public static readonly ColorRgba<float> Fuchsia = new ColorRgba<float>( 255, 0, 255, 255 ) / 255;
	public static readonly ColorRgba<float> Magenta = new ColorRgba<float>( 255, 0, 255, 255 ) / 255;
	public static readonly ColorRgba<float> SlateBlue = new ColorRgba<float>( 106, 90, 205, 255 ) / 255;
	public static readonly ColorRgba<float> MediumSlateBlue = new ColorRgba<float>( 123, 104, 238, 255 ) / 255;
	public static readonly ColorRgba<float> MediumOrchid = new ColorRgba<float>( 186, 85, 211, 255 ) / 255;
	public static readonly ColorRgba<float> MediumPurple = new ColorRgba<float>( 147, 112, 219, 255 ) / 255;
	public static readonly ColorRgba<float> Orchid = new ColorRgba<float>( 218, 112, 214, 255 ) / 255;
	public static readonly ColorRgba<float> Violet = new ColorRgba<float>( 238, 130, 238, 255 ) / 255;
	public static readonly ColorRgba<float> Plum = new ColorRgba<float>( 221, 160, 221, 255 ) / 255;
	public static readonly ColorRgba<float> Thistle = new ColorRgba<float>( 216, 191, 216, 255 ) / 255;
	public static readonly ColorRgba<float> Lavender = new ColorRgba<float>( 230, 230, 250, 255 ) / 255;
	public static readonly ColorRgba<float> MidnightBlue = new ColorRgba<float>( 25, 25, 112, 255 ) / 255;
	public static readonly ColorRgba<float> Navy = new ColorRgba<float>( 0, 0, 128, 255 ) / 255;
	public static readonly ColorRgba<float> DarkBlue = new ColorRgba<float>( 0, 0, 139, 255 ) / 255;
	public static readonly ColorRgba<float> MediumBlue = new ColorRgba<float>( 0, 0, 205, 255 ) / 255;
	public static readonly ColorRgba<float> Blue = new ColorRgba<float>( 0, 0, 255, 255 ) / 255;
	public static readonly ColorRgba<float> RoyalBlue = new ColorRgba<float>( 65, 105, 225, 255 ) / 255;
	public static readonly ColorRgba<float> SteelBlue = new ColorRgba<float>( 70, 130, 180, 255 ) / 255;
	public static readonly ColorRgba<float> DodgerBlue = new ColorRgba<float>( 30, 144, 255, 255 ) / 255;
	public static readonly ColorRgba<float> DeepSkyBlue = new ColorRgba<float>( 0, 191, 255, 255 ) / 255;
	public static readonly ColorRgba<float> CornflowerBlue = new ColorRgba<float>( 100, 149, 237, 255 ) / 255;
	public static readonly ColorRgba<float> SkyBlue = new ColorRgba<float>( 135, 206, 235, 255 ) / 255;
	public static readonly ColorRgba<float> LightSkyBlue = new ColorRgba<float>( 135, 206, 250, 255 ) / 255;
	public static readonly ColorRgba<float> LightSteelBlue = new ColorRgba<float>( 176, 196, 222, 255 ) / 255;
	public static readonly ColorRgba<float> LightBlue = new ColorRgba<float>( 173, 216, 230, 255 ) / 255;
	public static readonly ColorRgba<float> PowderBlue = new ColorRgba<float>( 176, 224, 230, 255 ) / 255;
	public static readonly ColorRgba<float> Teal = new ColorRgba<float>( 0, 128, 128, 255 ) / 255;
	public static readonly ColorRgba<float> DarkCyan = new ColorRgba<float>( 0, 139, 139, 255 ) / 255;
	public static readonly ColorRgba<float> LightSeaGreen = new ColorRgba<float>( 32, 178, 170, 255 ) / 255;
	public static readonly ColorRgba<float> CadetBlue = new ColorRgba<float>( 95, 158, 160, 255 ) / 255;
	public static readonly ColorRgba<float> DarkTurquoise = new ColorRgba<float>( 0, 206, 209, 255 ) / 255;
	public static readonly ColorRgba<float> MediumTurquoise = new ColorRgba<float>( 72, 209, 204, 255 ) / 255;
	public static readonly ColorRgba<float> Turquoise = new ColorRgba<float>( 64, 224, 208, 255 ) / 255;
	public static readonly ColorRgba<float> Aqua = new ColorRgba<float>( 0, 255, 255, 255 ) / 255;
	public static readonly ColorRgba<float> Cyan = new ColorRgba<float>( 0, 255, 255, 255 ) / 255;
	public static readonly ColorRgba<float> Aquamarine = new ColorRgba<float>( 127, 255, 212, 255 ) / 255;
	public static readonly ColorRgba<float> PaleTurquoise = new ColorRgba<float>( 175, 238, 238, 255 ) / 255;
	public static readonly ColorRgba<float> LightCyan = new ColorRgba<float>( 224, 255, 255, 255 ) / 255;
	public static readonly ColorRgba<float> DarkGreen = new ColorRgba<float>( 0, 100, 0, 255 ) / 255;
	public static readonly ColorRgba<float> Green = new ColorRgba<float>( 0, 128, 0, 255 ) / 255;
	public static readonly ColorRgba<float> DarkOliveGreen = new ColorRgba<float>( 85, 107, 47, 255 ) / 255;
	public static readonly ColorRgba<float> ForestGreen = new ColorRgba<float>( 34, 139, 34, 255 ) / 255;
	public static readonly ColorRgba<float> SeaGreen = new ColorRgba<float>( 46, 139, 87, 255 ) / 255;
	public static readonly ColorRgba<float> Olive = new ColorRgba<float>( 128, 128, 0, 255 ) / 255;
	public static readonly ColorRgba<float> OliveDrab = new ColorRgba<float>( 107, 142, 35, 255 ) / 255;
	public static readonly ColorRgba<float> MediumSeaGreen = new ColorRgba<float>( 60, 179, 113, 255 ) / 255;
	public static readonly ColorRgba<float> LimeGreen = new ColorRgba<float>( 50, 205, 50, 255 ) / 255;
	public static readonly ColorRgba<float> Lime = new ColorRgba<float>( 0, 255, 0, 255 ) / 255;
	public static readonly ColorRgba<float> SpringGreen = new ColorRgba<float>( 0, 255, 127, 255 ) / 255;
	public static readonly ColorRgba<float> MediumSpringGreen = new ColorRgba<float>( 0, 250, 154, 255 ) / 255;
	public static readonly ColorRgba<float> DarkSeaGreen = new ColorRgba<float>( 143, 188, 143, 255 ) / 255;
	public static readonly ColorRgba<float> MediumAquamarine = new ColorRgba<float>( 102, 205, 170, 255 ) / 255;
	public static readonly ColorRgba<float> YellowGreen = new ColorRgba<float>( 154, 205, 50, 255 ) / 255;
	public static readonly ColorRgba<float> LawnGreen = new ColorRgba<float>( 124, 252, 0, 255 ) / 255;
	public static readonly ColorRgba<float> Chartreuse = new ColorRgba<float>( 127, 255, 0, 255 ) / 255;
	public static readonly ColorRgba<float> LightGreen = new ColorRgba<float>( 144, 238, 144, 255 ) / 255;
	public static readonly ColorRgba<float> GreenYellow = new ColorRgba<float>( 173, 255, 47, 255 ) / 255;
	public static readonly ColorRgba<float> PaleGreen = new ColorRgba<float>( 152, 251, 152, 255 ) / 255;
	public static readonly ColorRgba<float> MistyRose = new ColorRgba<float>( 255, 228, 225, 255 ) / 255;
	public static readonly ColorRgba<float> AntiqueWhite = new ColorRgba<float>( 250, 235, 215, 255 ) / 255;
	public static readonly ColorRgba<float> Linen = new ColorRgba<float>( 250, 240, 230, 255 ) / 255;
	public static readonly ColorRgba<float> Beige = new ColorRgba<float>( 245, 245, 220, 255 ) / 255;
	public static readonly ColorRgba<float> WhiteSmoke = new ColorRgba<float>( 245, 245, 245, 255 ) / 255;
	public static readonly ColorRgba<float> LavenderBlush = new ColorRgba<float>( 255, 240, 245, 255 ) / 255;
	public static readonly ColorRgba<float> OldLace = new ColorRgba<float>( 253, 245, 230, 255 ) / 255;
	public static readonly ColorRgba<float> AliceBlue = new ColorRgba<float>( 240, 248, 255, 255 ) / 255;
	public static readonly ColorRgba<float> Seashell = new ColorRgba<float>( 255, 245, 238, 255 ) / 255;
	public static readonly ColorRgba<float> GhostWhite = new ColorRgba<float>( 248, 248, 255, 255 ) / 255;
	public static readonly ColorRgba<float> Honeydew = new ColorRgba<float>( 240, 255, 240, 255 ) / 255;
	public static readonly ColorRgba<float> FloralWhite = new ColorRgba<float>( 255, 250, 240, 255 ) / 255;
	public static readonly ColorRgba<float> Azure = new ColorRgba<float>( 240, 255, 255, 255 ) / 255;
	public static readonly ColorRgba<float> MintCream = new ColorRgba<float>( 245, 255, 250, 255 ) / 255;
	public static readonly ColorRgba<float> Snow = new ColorRgba<float>( 255, 250, 250, 255 ) / 255;
	public static readonly ColorRgba<float> Ivory = new ColorRgba<float>( 255, 255, 240, 255 ) / 255;
	public static readonly ColorRgba<float> White = new ColorRgba<float>( 255, 255, 255, 255 ) / 255;
	public static readonly ColorRgba<float> Black = new ColorRgba<float>( 0, 0, 0, 255 ) / 255;
	public static readonly ColorRgba<float> DarkSlateGray = new ColorRgba<float>( 47, 79, 79, 255 ) / 255;
	public static readonly ColorRgba<float> DimGray = new ColorRgba<float>( 105, 105, 105, 255 ) / 255;
	public static readonly ColorRgba<float> SlateGray = new ColorRgba<float>( 112, 128, 144, 255 ) / 255;
	public static readonly ColorRgba<float> Gray = new ColorRgba<float>( 128, 128, 128, 255 ) / 255;
	public static readonly ColorRgba<float> LightSlateGray = new ColorRgba<float>( 119, 136, 153, 255 ) / 255;
	public static readonly ColorRgba<float> DarkGray = new ColorRgba<float>( 169, 169, 169, 255 ) / 255;
	public static readonly ColorRgba<float> Silver = new ColorRgba<float>( 192, 192, 192, 255 ) / 255;
	public static readonly ColorRgba<float> LightGray = new ColorRgba<float>( 211, 211, 211, 255 ) / 255;
	public static readonly ColorRgba<float> Gainsboro = new ColorRgba<float>( 220, 220, 220, 255 ) / 255;

	public static ColorRgba<byte> ToByte<T> ( this ColorRgba<T> color ) where T : IFloatingPoint<T> {
		var _255 = T.CreateChecked( 255 );
		return new() {
			R = byte.CreateTruncating(color.R * _255),
			G = byte.CreateTruncating(color.G * _255),
			B = byte.CreateTruncating(color.B * _255),
			A = byte.CreateTruncating(color.A * _255)
		};
	}

	public static ColorRgba<T> Interpolate<T, TTime> ( this ColorRgba<T> from, ColorRgba<T> to, TTime time ) where T : INumber<T>, IFloatingPointIeee754<T> where TTime : INumber<TTime>, IMultiplyOperators<TTime, T, T> {
		return new() {
			R = T.Sqrt( (TTime.One - time) * (from.R * from.R) + time * (to.R * to.R) ),
			G = T.Sqrt( (TTime.One - time) * (from.G * from.G) + time * (to.G * to.G) ),
			B = T.Sqrt( (TTime.One - time) * (from.B * from.B) + time * (to.B * to.B) ),
			A = (TTime.One - time) * from.A + time * to.A
		};
	}
}