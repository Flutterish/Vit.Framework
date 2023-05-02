using System.Numerics;

namespace Vit.Framework.Graphics;

public struct ColorRgb<T> where T : INumber<T> {
	public T R;
	public T G;
	public T B;

	public ColorRgb ( T r, T g, T b ) {
		R = r;
		G = g;
		B = b;
	}

	public static ColorRgb<T> operator / ( ColorRgb<T> color, T scalar ) {
		return new() {
			R = color.R / scalar,
			G = color.G / scalar,
			B = color.B / scalar
		};
	}

	public static bool operator == ( ColorRgb<T> left, ColorRgb<T> right ) {
		return left.R == right.R
			&& left.G == right.G
			&& left.B == right.B;
	}

	public static bool operator != ( ColorRgb<T> left, ColorRgb<T> right ) {
		return left.R != right.R
			|| left.G != right.G
			|| left.B != right.B;
	}

	public override string ToString () {
		if ( typeof(T) == typeof(byte) ) {
			var _255 = T.CreateChecked( 255 );
			return $"#{R:X2}{G:X2}{B:X2}";
		}
		else if ( typeof(T).IsAssignableTo(typeof(IFloatingPoint<>)) ) {
			var _255 = T.CreateChecked( 255 );
			var (r, g, b) = (byte.CreateTruncating(R * _255),byte.CreateTruncating(G * _255), byte.CreateTruncating(B * _255));
			return $"#{r:X2}{g:X2}{b:X2}";
		}
		else {
			return $"({R}, {G}, {B})";
		}
	}
}

public static class ColorRgb {
	public static readonly ColorRgb<float> MediumVioletRed = new ColorRgb<float>( 199, 21, 133 ) / 255;
	public static readonly ColorRgb<float> DeepPink = new ColorRgb<float>( 255, 20, 147 ) / 255;
	public static readonly ColorRgb<float> PaleVioletRed = new ColorRgb<float>( 219, 112, 147 ) / 255;
	public static readonly ColorRgb<float> HotPink = new ColorRgb<float>( 255, 105, 180 ) / 255;
	public static readonly ColorRgb<float> LightPink = new ColorRgb<float>( 255, 182, 193 ) / 255;
	public static readonly ColorRgb<float> Pink = new ColorRgb<float>( 255, 192, 203 ) / 255;
	public static readonly ColorRgb<float> DarkRed = new ColorRgb<float>( 139, 0, 0 ) / 255;
	public static readonly ColorRgb<float> Red = new ColorRgb<float>( 255, 0, 0 ) / 255;
	public static readonly ColorRgb<float> Firebrick = new ColorRgb<float>( 178, 34, 34 ) / 255;
	public static readonly ColorRgb<float> Crimson = new ColorRgb<float>( 220, 20, 60 ) / 255;
	public static readonly ColorRgb<float> IndianRed = new ColorRgb<float>( 205, 92, 92 ) / 255;
	public static readonly ColorRgb<float> LightCoral = new ColorRgb<float>( 240, 128, 128 ) / 255;
	public static readonly ColorRgb<float> Salmon = new ColorRgb<float>( 250, 128, 114 ) / 255;
	public static readonly ColorRgb<float> DarkSalmon = new ColorRgb<float>( 233, 150, 122 ) / 255;
	public static readonly ColorRgb<float> LightSalmon = new ColorRgb<float>( 255, 160, 122 ) / 255;
	public static readonly ColorRgb<float> OrangeRed = new ColorRgb<float>( 255, 69, 0 ) / 255;
	public static readonly ColorRgb<float> Tomato = new ColorRgb<float>( 255, 99, 71 ) / 255;
	public static readonly ColorRgb<float> DarkOrange = new ColorRgb<float>( 255, 140, 0 ) / 255;
	public static readonly ColorRgb<float> Coral = new ColorRgb<float>( 255, 127, 80 ) / 255;
	public static readonly ColorRgb<float> Orange = new ColorRgb<float>( 255, 165, 0 ) / 255;
	public static readonly ColorRgb<float> DarkKhaki = new ColorRgb<float>( 189, 183, 107 ) / 255;
	public static readonly ColorRgb<float> Gold = new ColorRgb<float>( 255, 215, 0 ) / 255;
	public static readonly ColorRgb<float> Khaki = new ColorRgb<float>( 240, 230, 140 ) / 255;
	public static readonly ColorRgb<float> PeachPuff = new ColorRgb<float>( 255, 218, 185 ) / 255;
	public static readonly ColorRgb<float> Yellow = new ColorRgb<float>( 255, 255, 0 ) / 255;
	public static readonly ColorRgb<float> PaleGoldenrod = new ColorRgb<float>( 238, 232, 170 ) / 255;
	public static readonly ColorRgb<float> Moccasin = new ColorRgb<float>( 255, 228, 181 ) / 255;
	public static readonly ColorRgb<float> PapayaWhip = new ColorRgb<float>( 255, 239, 213 ) / 255;
	public static readonly ColorRgb<float> LightGoldenrodYellow = new ColorRgb<float>( 250, 250, 210 ) / 255;
	public static readonly ColorRgb<float> LemonChiffon = new ColorRgb<float>( 255, 250, 205 ) / 255;
	public static readonly ColorRgb<float> LightYellow = new ColorRgb<float>( 255, 255, 224 ) / 255;
	public static readonly ColorRgb<float> Maroon = new ColorRgb<float>( 128, 0, 0 ) / 255;
	public static readonly ColorRgb<float> Brown = new ColorRgb<float>( 165, 42, 42 ) / 255;
	public static readonly ColorRgb<float> SaddleBrown = new ColorRgb<float>( 139, 69, 19 ) / 255;
	public static readonly ColorRgb<float> Sienna = new ColorRgb<float>( 160, 82, 45 ) / 255;
	public static readonly ColorRgb<float> Chocolate = new ColorRgb<float>( 210, 105, 30 ) / 255;
	public static readonly ColorRgb<float> DarkGoldenrod = new ColorRgb<float>( 184, 134, 11 ) / 255;
	public static readonly ColorRgb<float> Peru = new ColorRgb<float>( 205, 133, 63 ) / 255;
	public static readonly ColorRgb<float> RosyBrown = new ColorRgb<float>( 188, 143, 143 ) / 255;
	public static readonly ColorRgb<float> Goldenrod = new ColorRgb<float>( 218, 165, 32 ) / 255;
	public static readonly ColorRgb<float> SandyBrown = new ColorRgb<float>( 244, 164, 96 ) / 255;
	public static readonly ColorRgb<float> Tan = new ColorRgb<float>( 210, 180, 140 ) / 255;
	public static readonly ColorRgb<float> Burlywood = new ColorRgb<float>( 222, 184, 135 ) / 255;
	public static readonly ColorRgb<float> Wheat = new ColorRgb<float>( 245, 222, 179 ) / 255;
	public static readonly ColorRgb<float> NavajoWhite = new ColorRgb<float>( 255, 222, 173 ) / 255;
	public static readonly ColorRgb<float> Bisque = new ColorRgb<float>( 255, 228, 196 ) / 255;
	public static readonly ColorRgb<float> BlanchedAlmond = new ColorRgb<float>( 255, 235, 205 ) / 255;
	public static readonly ColorRgb<float> Cornsilk = new ColorRgb<float>( 255, 248, 220 ) / 255;
	public static readonly ColorRgb<float> Indigo = new ColorRgb<float>( 75, 0, 130 ) / 255;
	public static readonly ColorRgb<float> Purple = new ColorRgb<float>( 128, 0, 128 ) / 255;
	public static readonly ColorRgb<float> DarkMagenta = new ColorRgb<float>( 139, 0, 139 ) / 255;
	public static readonly ColorRgb<float> DarkViolet = new ColorRgb<float>( 148, 0, 211 ) / 255;
	public static readonly ColorRgb<float> DarkSlateBlue = new ColorRgb<float>( 72, 61, 139 ) / 255;
	public static readonly ColorRgb<float> BlueViolet = new ColorRgb<float>( 138, 43, 226 ) / 255;
	public static readonly ColorRgb<float> DarkOrchid = new ColorRgb<float>( 153, 50, 204 ) / 255;
	public static readonly ColorRgb<float> Fuchsia = new ColorRgb<float>( 255, 0, 255 ) / 255;
	public static readonly ColorRgb<float> Magenta = new ColorRgb<float>( 255, 0, 255 ) / 255;
	public static readonly ColorRgb<float> SlateBlue = new ColorRgb<float>( 106, 90, 205 ) / 255;
	public static readonly ColorRgb<float> MediumSlateBlue = new ColorRgb<float>( 123, 104, 238 ) / 255;
	public static readonly ColorRgb<float> MediumOrchid = new ColorRgb<float>( 186, 85, 211 ) / 255;
	public static readonly ColorRgb<float> MediumPurple = new ColorRgb<float>( 147, 112, 219 ) / 255;
	public static readonly ColorRgb<float> Orchid = new ColorRgb<float>( 218, 112, 214 ) / 255;
	public static readonly ColorRgb<float> Violet = new ColorRgb<float>( 238, 130, 238 ) / 255;
	public static readonly ColorRgb<float> Plum = new ColorRgb<float>( 221, 160, 221 ) / 255;
	public static readonly ColorRgb<float> Thistle = new ColorRgb<float>( 216, 191, 216 ) / 255;
	public static readonly ColorRgb<float> Lavender = new ColorRgb<float>( 230, 230, 250 ) / 255;
	public static readonly ColorRgb<float> MidnightBlue = new ColorRgb<float>( 25, 25, 112 ) / 255;
	public static readonly ColorRgb<float> Navy = new ColorRgb<float>( 0, 0, 128 ) / 255;
	public static readonly ColorRgb<float> DarkBlue = new ColorRgb<float>( 0, 0, 139 ) / 255;
	public static readonly ColorRgb<float> MediumBlue = new ColorRgb<float>( 0, 0, 205 ) / 255;
	public static readonly ColorRgb<float> Blue = new ColorRgb<float>( 0, 0, 255 ) / 255;
	public static readonly ColorRgb<float> RoyalBlue = new ColorRgb<float>( 65, 105, 225 ) / 255;
	public static readonly ColorRgb<float> SteelBlue = new ColorRgb<float>( 70, 130, 180 ) / 255;
	public static readonly ColorRgb<float> DodgerBlue = new ColorRgb<float>( 30, 144, 255 ) / 255;
	public static readonly ColorRgb<float> DeepSkyBlue = new ColorRgb<float>( 0, 191, 255 ) / 255;
	public static readonly ColorRgb<float> CornflowerBlue = new ColorRgb<float>( 100, 149, 237 ) / 255;
	public static readonly ColorRgb<float> SkyBlue = new ColorRgb<float>( 135, 206, 235 ) / 255;
	public static readonly ColorRgb<float> LightSkyBlue = new ColorRgb<float>( 135, 206, 250 ) / 255;
	public static readonly ColorRgb<float> LightSteelBlue = new ColorRgb<float>( 176, 196, 222 ) / 255;
	public static readonly ColorRgb<float> LightBlue = new ColorRgb<float>( 173, 216, 230 ) / 255;
	public static readonly ColorRgb<float> PowderBlue = new ColorRgb<float>( 176, 224, 230 ) / 255;
	public static readonly ColorRgb<float> Teal = new ColorRgb<float>( 0, 128, 128 ) / 255;
	public static readonly ColorRgb<float> DarkCyan = new ColorRgb<float>( 0, 139, 139 ) / 255;
	public static readonly ColorRgb<float> LightSeaGreen = new ColorRgb<float>( 32, 178, 170 ) / 255;
	public static readonly ColorRgb<float> CadetBlue = new ColorRgb<float>( 95, 158, 160 ) / 255;
	public static readonly ColorRgb<float> DarkTurquoise = new ColorRgb<float>( 0, 206, 209 ) / 255;
	public static readonly ColorRgb<float> MediumTurquoise = new ColorRgb<float>( 72, 209, 204 ) / 255;
	public static readonly ColorRgb<float> Turquoise = new ColorRgb<float>( 64, 224, 208 ) / 255;
	public static readonly ColorRgb<float> Aqua = new ColorRgb<float>( 0, 255, 255 ) / 255;
	public static readonly ColorRgb<float> Cyan = new ColorRgb<float>( 0, 255, 255 ) / 255;
	public static readonly ColorRgb<float> Aquamarine = new ColorRgb<float>( 127, 255, 212 ) / 255;
	public static readonly ColorRgb<float> PaleTurquoise = new ColorRgb<float>( 175, 238, 238 ) / 255;
	public static readonly ColorRgb<float> LightCyan = new ColorRgb<float>( 224, 255, 255 ) / 255;
	public static readonly ColorRgb<float> DarkGreen = new ColorRgb<float>( 0, 100, 0 ) / 255;
	public static readonly ColorRgb<float> Green = new ColorRgb<float>( 0, 128, 0 ) / 255;
	public static readonly ColorRgb<float> DarkOliveGreen = new ColorRgb<float>( 85, 107, 47 ) / 255;
	public static readonly ColorRgb<float> ForestGreen = new ColorRgb<float>( 34, 139, 34 ) / 255;
	public static readonly ColorRgb<float> SeaGreen = new ColorRgb<float>( 46, 139, 87 ) / 255;
	public static readonly ColorRgb<float> Olive = new ColorRgb<float>( 128, 128, 0 ) / 255;
	public static readonly ColorRgb<float> OliveDrab = new ColorRgb<float>( 107, 142, 35 ) / 255;
	public static readonly ColorRgb<float> MediumSeaGreen = new ColorRgb<float>( 60, 179, 113 ) / 255;
	public static readonly ColorRgb<float> LimeGreen = new ColorRgb<float>( 50, 205, 50 ) / 255;
	public static readonly ColorRgb<float> Lime = new ColorRgb<float>( 0, 255, 0 ) / 255;
	public static readonly ColorRgb<float> SpringGreen = new ColorRgb<float>( 0, 255, 127 ) / 255;
	public static readonly ColorRgb<float> MediumSpringGreen = new ColorRgb<float>( 0, 250, 154 ) / 255;
	public static readonly ColorRgb<float> DarkSeaGreen = new ColorRgb<float>( 143, 188, 143 ) / 255;
	public static readonly ColorRgb<float> MediumAquamarine = new ColorRgb<float>( 102, 205, 170 ) / 255;
	public static readonly ColorRgb<float> YellowGreen = new ColorRgb<float>( 154, 205, 50 ) / 255;
	public static readonly ColorRgb<float> LawnGreen = new ColorRgb<float>( 124, 252, 0 ) / 255;
	public static readonly ColorRgb<float> Chartreuse = new ColorRgb<float>( 127, 255, 0 ) / 255;
	public static readonly ColorRgb<float> LightGreen = new ColorRgb<float>( 144, 238, 144 ) / 255;
	public static readonly ColorRgb<float> GreenYellow = new ColorRgb<float>( 173, 255, 47 ) / 255;
	public static readonly ColorRgb<float> PaleGreen = new ColorRgb<float>( 152, 251, 152 ) / 255;
	public static readonly ColorRgb<float> MistyRose = new ColorRgb<float>( 255, 228, 225 ) / 255;
	public static readonly ColorRgb<float> AntiqueWhite = new ColorRgb<float>( 250, 235, 215 ) / 255;
	public static readonly ColorRgb<float> Linen = new ColorRgb<float>( 250, 240, 230 ) / 255;
	public static readonly ColorRgb<float> Beige = new ColorRgb<float>( 245, 245, 220 ) / 255;
	public static readonly ColorRgb<float> WhiteSmoke = new ColorRgb<float>( 245, 245, 245 ) / 255;
	public static readonly ColorRgb<float> LavenderBlush = new ColorRgb<float>( 255, 240, 245 ) / 255;
	public static readonly ColorRgb<float> OldLace = new ColorRgb<float>( 253, 245, 230 ) / 255;
	public static readonly ColorRgb<float> AliceBlue = new ColorRgb<float>( 240, 248, 255 ) / 255;
	public static readonly ColorRgb<float> Seashell = new ColorRgb<float>( 255, 245, 238 ) / 255;
	public static readonly ColorRgb<float> GhostWhite = new ColorRgb<float>( 248, 248, 255 ) / 255;
	public static readonly ColorRgb<float> Honeydew = new ColorRgb<float>( 240, 255, 240 ) / 255;
	public static readonly ColorRgb<float> FloralWhite = new ColorRgb<float>( 255, 250, 240 ) / 255;
	public static readonly ColorRgb<float> Azure = new ColorRgb<float>( 240, 255, 255 ) / 255;
	public static readonly ColorRgb<float> MintCream = new ColorRgb<float>( 245, 255, 250 ) / 255;
	public static readonly ColorRgb<float> Snow = new ColorRgb<float>( 255, 250, 250 ) / 255;
	public static readonly ColorRgb<float> Ivory = new ColorRgb<float>( 255, 255, 240 ) / 255;
	public static readonly ColorRgb<float> White = new ColorRgb<float>( 255, 255, 255 ) / 255;
	public static readonly ColorRgb<float> Black = new ColorRgb<float>( 0, 0, 0 ) / 255;
	public static readonly ColorRgb<float> DarkSlateGray = new ColorRgb<float>( 47, 79, 79 ) / 255;
	public static readonly ColorRgb<float> DimGray = new ColorRgb<float>( 105, 105, 105 ) / 255;
	public static readonly ColorRgb<float> SlateGray = new ColorRgb<float>( 112, 128, 144 ) / 255;
	public static readonly ColorRgb<float> Gray = new ColorRgb<float>( 128, 128, 128 ) / 255;
	public static readonly ColorRgb<float> LightSlateGray = new ColorRgb<float>( 119, 136, 153 ) / 255;
	public static readonly ColorRgb<float> DarkGray = new ColorRgb<float>( 169, 169, 169 ) / 255;
	public static readonly ColorRgb<float> Silver = new ColorRgb<float>( 192, 192, 192 ) / 255;
	public static readonly ColorRgb<float> LightGray = new ColorRgb<float>( 211, 211, 211 ) / 255;
	public static readonly ColorRgb<float> Gainsboro = new ColorRgb<float>( 220, 220, 220 ) / 255;

	public static ColorRgb<byte> ToByte<T> ( this ColorRgb<T> color ) where T : IFloatingPoint<T> {
		var _255 = T.CreateChecked( 255 );
		return new() {
			R = byte.CreateTruncating(color.R * _255),
			G = byte.CreateTruncating(color.G * _255),
			B = byte.CreateTruncating(color.B * _255)
		};
	}
}