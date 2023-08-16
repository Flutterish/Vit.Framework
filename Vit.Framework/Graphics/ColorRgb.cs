using System.Numerics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics;

/// <summary>
/// A color represented in the standard RGB model (sRGB).
/// </summary>
/// <remarks>
/// The RBG values are linear in perceived brightness. <br/>
/// See: <see href="https://blog.johnnovak.net/2016/09/21/what-every-coder-should-know-about-gamma/#gradients"/>
/// </remarks>
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

/// <inheritdoc cref="ColorRgb{T}"/>
public static class ColorRgb {
	public static readonly ColorRgb<float> MediumVioletRed = (new LinearColorRgb<float>( 199, 21, 133 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DeepPink = (new LinearColorRgb<float>( 255, 20, 147 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> PaleVioletRed = (new LinearColorRgb<float>( 219, 112, 147 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> HotPink = (new LinearColorRgb<float>( 255, 105, 180 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LightPink = (new LinearColorRgb<float>( 255, 182, 193 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Pink = (new LinearColorRgb<float>( 255, 192, 203 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkRed = (new LinearColorRgb<float>( 139, 0, 0 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Red = (new LinearColorRgb<float>( 255, 0, 0 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Firebrick = (new LinearColorRgb<float>( 178, 34, 34 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Crimson = (new LinearColorRgb<float>( 220, 20, 60 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> IndianRed = (new LinearColorRgb<float>( 205, 92, 92 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LightCoral = (new LinearColorRgb<float>( 240, 128, 128 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Salmon = (new LinearColorRgb<float>( 250, 128, 114 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkSalmon = (new LinearColorRgb<float>( 233, 150, 122 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LightSalmon = (new LinearColorRgb<float>( 255, 160, 122 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> OrangeRed = (new LinearColorRgb<float>( 255, 69, 0 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Tomato = (new LinearColorRgb<float>( 255, 99, 71 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkOrange = (new LinearColorRgb<float>( 255, 140, 0 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Coral = (new LinearColorRgb<float>( 255, 127, 80 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Orange = (new LinearColorRgb<float>( 255, 165, 0 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkKhaki = (new LinearColorRgb<float>( 189, 183, 107 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Gold = (new LinearColorRgb<float>( 255, 215, 0 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Khaki = (new LinearColorRgb<float>( 240, 230, 140 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> PeachPuff = (new LinearColorRgb<float>( 255, 218, 185 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Yellow = (new LinearColorRgb<float>( 255, 255, 0 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> PaleGoldenrod = (new LinearColorRgb<float>( 238, 232, 170 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Moccasin = (new LinearColorRgb<float>( 255, 228, 181 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> PapayaWhip = (new LinearColorRgb<float>( 255, 239, 213 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LightGoldenrodYellow = (new LinearColorRgb<float>( 250, 250, 210 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LemonChiffon = (new LinearColorRgb<float>( 255, 250, 205 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LightYellow = (new LinearColorRgb<float>( 255, 255, 224 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Maroon = (new LinearColorRgb<float>( 128, 0, 0 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Brown = (new LinearColorRgb<float>( 165, 42, 42 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> SaddleBrown = (new LinearColorRgb<float>( 139, 69, 19 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Sienna = (new LinearColorRgb<float>( 160, 82, 45 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Chocolate = (new LinearColorRgb<float>( 210, 105, 30 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkGoldenrod = (new LinearColorRgb<float>( 184, 134, 11 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Peru = (new LinearColorRgb<float>( 205, 133, 63 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> RosyBrown = (new LinearColorRgb<float>( 188, 143, 143 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Goldenrod = (new LinearColorRgb<float>( 218, 165, 32 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> SandyBrown = (new LinearColorRgb<float>( 244, 164, 96 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Tan = (new LinearColorRgb<float>( 210, 180, 140 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Burlywood = (new LinearColorRgb<float>( 222, 184, 135 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Wheat = (new LinearColorRgb<float>( 245, 222, 179 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> NavajoWhite = (new LinearColorRgb<float>( 255, 222, 173 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Bisque = (new LinearColorRgb<float>( 255, 228, 196 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> BlanchedAlmond = (new LinearColorRgb<float>( 255, 235, 205 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Cornsilk = (new LinearColorRgb<float>( 255, 248, 220 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Indigo = (new LinearColorRgb<float>( 75, 0, 130 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Purple = (new LinearColorRgb<float>( 128, 0, 128 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkMagenta = (new LinearColorRgb<float>( 139, 0, 139 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkViolet = (new LinearColorRgb<float>( 148, 0, 211 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkSlateBlue = (new LinearColorRgb<float>( 72, 61, 139 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> BlueViolet = (new LinearColorRgb<float>( 138, 43, 226 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkOrchid = (new LinearColorRgb<float>( 153, 50, 204 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Fuchsia = (new LinearColorRgb<float>( 255, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Magenta = (new LinearColorRgb<float>( 255, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> SlateBlue = (new LinearColorRgb<float>( 106, 90, 205 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> MediumSlateBlue = (new LinearColorRgb<float>( 123, 104, 238 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> MediumOrchid = (new LinearColorRgb<float>( 186, 85, 211 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> MediumPurple = (new LinearColorRgb<float>( 147, 112, 219 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Orchid = (new LinearColorRgb<float>( 218, 112, 214 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Violet = (new LinearColorRgb<float>( 238, 130, 238 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Plum = (new LinearColorRgb<float>( 221, 160, 221 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Thistle = (new LinearColorRgb<float>( 216, 191, 216 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Lavender = (new LinearColorRgb<float>( 230, 230, 250 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> MidnightBlue = (new LinearColorRgb<float>( 25, 25, 112 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Navy = (new LinearColorRgb<float>( 0, 0, 128 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkBlue = (new LinearColorRgb<float>( 0, 0, 139 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> MediumBlue = (new LinearColorRgb<float>( 0, 0, 205 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Blue = (new LinearColorRgb<float>( 0, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> RoyalBlue = (new LinearColorRgb<float>( 65, 105, 225 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> SteelBlue = (new LinearColorRgb<float>( 70, 130, 180 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DodgerBlue = (new LinearColorRgb<float>( 30, 144, 255 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DeepSkyBlue = (new LinearColorRgb<float>( 0, 191, 255 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> CornflowerBlue = (new LinearColorRgb<float>( 100, 149, 237 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> SkyBlue = (new LinearColorRgb<float>( 135, 206, 235 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LightSkyBlue = (new LinearColorRgb<float>( 135, 206, 250 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LightSteelBlue = (new LinearColorRgb<float>( 176, 196, 222 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LightBlue = (new LinearColorRgb<float>( 173, 216, 230 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> PowderBlue = (new LinearColorRgb<float>( 176, 224, 230 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Teal = (new LinearColorRgb<float>( 0, 128, 128 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkCyan = (new LinearColorRgb<float>( 0, 139, 139 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LightSeaGreen = (new LinearColorRgb<float>( 32, 178, 170 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> CadetBlue = (new LinearColorRgb<float>( 95, 158, 160 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkTurquoise = (new LinearColorRgb<float>( 0, 206, 209 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> MediumTurquoise = (new LinearColorRgb<float>( 72, 209, 204 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Turquoise = (new LinearColorRgb<float>( 64, 224, 208 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Aqua = (new LinearColorRgb<float>( 0, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Cyan = (new LinearColorRgb<float>( 0, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Aquamarine = (new LinearColorRgb<float>( 127, 255, 212 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> PaleTurquoise = (new LinearColorRgb<float>( 175, 238, 238 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LightCyan = (new LinearColorRgb<float>( 224, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkGreen = (new LinearColorRgb<float>( 0, 100, 0 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Green = (new LinearColorRgb<float>( 0, 128, 0 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkOliveGreen = (new LinearColorRgb<float>( 85, 107, 47 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> ForestGreen = (new LinearColorRgb<float>( 34, 139, 34 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> SeaGreen = (new LinearColorRgb<float>( 46, 139, 87 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Olive = (new LinearColorRgb<float>( 128, 128, 0 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> OliveDrab = (new LinearColorRgb<float>( 107, 142, 35 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> MediumSeaGreen = (new LinearColorRgb<float>( 60, 179, 113 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LimeGreen = (new LinearColorRgb<float>( 50, 205, 50 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Lime = (new LinearColorRgb<float>( 0, 255, 0 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> SpringGreen = (new LinearColorRgb<float>( 0, 255, 127 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> MediumSpringGreen = (new LinearColorRgb<float>( 0, 250, 154 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkSeaGreen = (new LinearColorRgb<float>( 143, 188, 143 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> MediumAquamarine = (new LinearColorRgb<float>( 102, 205, 170 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> YellowGreen = (new LinearColorRgb<float>( 154, 205, 50 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LawnGreen = (new LinearColorRgb<float>( 124, 252, 0 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Chartreuse = (new LinearColorRgb<float>( 127, 255, 0 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LightGreen = (new LinearColorRgb<float>( 144, 238, 144 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> GreenYellow = (new LinearColorRgb<float>( 173, 255, 47 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> PaleGreen = (new LinearColorRgb<float>( 152, 251, 152 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> MistyRose = (new LinearColorRgb<float>( 255, 228, 225 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> AntiqueWhite = (new LinearColorRgb<float>( 250, 235, 215 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Linen = (new LinearColorRgb<float>( 250, 240, 230 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Beige = (new LinearColorRgb<float>( 245, 245, 220 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> WhiteSmoke = (new LinearColorRgb<float>( 245, 245, 245 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LavenderBlush = (new LinearColorRgb<float>( 255, 240, 245 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> OldLace = (new LinearColorRgb<float>( 253, 245, 230 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> AliceBlue = (new LinearColorRgb<float>( 240, 248, 255 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Seashell = (new LinearColorRgb<float>( 255, 245, 238 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> GhostWhite = (new LinearColorRgb<float>( 248, 248, 255 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Honeydew = (new LinearColorRgb<float>( 240, 255, 240 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> FloralWhite = (new LinearColorRgb<float>( 255, 250, 240 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Azure = (new LinearColorRgb<float>( 240, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> MintCream = (new LinearColorRgb<float>( 245, 255, 250 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Snow = (new LinearColorRgb<float>( 255, 250, 250 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Ivory = (new LinearColorRgb<float>( 255, 255, 240 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> White = (new LinearColorRgb<float>( 255, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Black = (new LinearColorRgb<float>( 0, 0, 0 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkSlateGray = (new LinearColorRgb<float>( 47, 79, 79 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DimGray = (new LinearColorRgb<float>( 105, 105, 105 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> SlateGray = (new LinearColorRgb<float>( 112, 128, 144 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Gray = (new LinearColorRgb<float>( 128, 128, 128 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LightSlateGray = (new LinearColorRgb<float>( 119, 136, 153 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> DarkGray = (new LinearColorRgb<float>( 169, 169, 169 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Silver = (new LinearColorRgb<float>( 192, 192, 192 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> LightGray = (new LinearColorRgb<float>( 211, 211, 211 ) / 255).ToSRGB();
	public static readonly ColorRgb<float> Gainsboro = (new LinearColorRgb<float>( 220, 220, 220 ) / 255).ToSRGB();

	public static ColorRgb<byte> ToByte<T> ( this ColorRgb<T> color ) where T : IFloatingPoint<T> {
		var _255 = T.CreateChecked( 255 );
		return new() {
			R = byte.CreateTruncating(color.R * _255),
			G = byte.CreateTruncating(color.G * _255),
			B = byte.CreateTruncating(color.B * _255)
		};
	}

	public static LinearColorRgb<T> ToLinear<T> ( this ColorRgb<T> color ) where T : IFloatingPointIeee754<T> {
		T gamma = T.CreateSaturating( 1 / 2.2 );
		return new() {
			R = T.Pow( color.R, gamma ),
			G = T.Pow( color.G, gamma ),
			B = T.Pow( color.B, gamma )
		};
	}

	public static ColorRgb<T> ToSRGB<T> ( this LinearColorRgb<T> color ) where T : IFloatingPointIeee754<T> {
		T gamma = T.CreateSaturating( 2.2 );
		return new() {
			R = T.Pow( color.R, gamma ),
			G = T.Pow( color.G, gamma ),
			B = T.Pow( color.B, gamma )
		};
	}

	public static ColorRgb<T> Interpolate<T, TTime> ( this ColorRgb<T> from, ColorRgb<T> to, TTime time ) where T : INumber<T>, IFloatingPointIeee754<T> where TTime : INumber<TTime>, IMultiplyOperators<TTime, T, T> {
		return from.ToLinear().Interpolate( to.ToLinear(), time ).ToSRGB();
	}

	public static LinearColorRgb<T> Interpolate<T, TTime> ( this LinearColorRgb<T> from, LinearColorRgb<T> to, TTime time ) where T : INumber<T>, IFloatingPointIeee754<T> where TTime : INumber<TTime>, IMultiplyOperators<TTime, T, T> {
		return new() {
			R = from.R.Lerp( to.R, time ),
			G = from.G.Lerp( to.G, time ),
			B = from.B.Lerp( to.B, time )
		};
	}
}