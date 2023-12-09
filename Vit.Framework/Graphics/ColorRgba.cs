using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics;

/// <summary>
/// A color represented in the linear RGB model with a <b>premultiplied alpha</b> component.
/// </summary>
/// <remarks>
/// The RBG values are linear in absolute brightness. <br/>
/// See: <see href="https://blog.johnnovak.net/2016/09/21/what-every-coder-should-know-about-gamma/#gradients"/>
/// </remarks>
public struct ColorRgba<T> : IUnlabelledColor<T>, IEqualityOperators<ColorRgba<T>, ColorRgba<T>, bool> where T : INumber<T> {
	public ReadOnlySpan<T> AsSpan () => MemoryMarshal.CreateReadOnlySpan( ref R, 4 );
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
		if ( typeof( T ) == typeof( byte ) ) {
			var _255 = T.CreateChecked( 255 );
			return $"#{R:X2}{G:X2}{B:X2}{(A == _255 ? "" : $"{A:X2}")}";
		}
		else if ( typeof( T ).IsAssignableTo( typeof( IFloatingPoint<> ) ) ) {
			var _255 = T.CreateChecked( 255 );
			var (r, g, b, a) = (byte.CreateTruncating( R * _255 ), byte.CreateTruncating( G * _255 ), byte.CreateTruncating( B * _255 ), byte.CreateTruncating( A * _255 ));
			return $"#{r:X2}{g:X2}{b:X2}{(a == 255 ? "" : $"{a:X2}")}";
		}
		else {
			return $"({R}, {G}, {B}, {A})";
		}
	}
}

/// <inheritdoc cref="ColorRgba{T}"/>
public static class ColorRgba {
	public static readonly ColorRgba<float> MediumVioletRed = new ColorRgba<float>( 199 / 255f, 21 / 255f, 133 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DeepPink = new ColorRgba<float>( 255 / 255f, 20 / 255f, 147 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> PaleVioletRed = new ColorRgba<float>( 219 / 255f, 112 / 255f, 147 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> HotPink = new ColorRgba<float>( 255 / 255f, 105 / 255f, 180 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LightPink = new ColorRgba<float>( 255 / 255f, 182 / 255f, 193 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Pink = new ColorRgba<float>( 255 / 255f, 192 / 255f, 203 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkRed = new ColorRgba<float>( 139 / 255f, 0 / 255f, 0 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Red = new ColorRgba<float>( 255 / 255f, 0 / 255f, 0 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Firebrick = new ColorRgba<float>( 178 / 255f, 34 / 255f, 34 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Crimson = new ColorRgba<float>( 220 / 255f, 20 / 255f, 60 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> IndianRed = new ColorRgba<float>( 205 / 255f, 92 / 255f, 92 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LightCoral = new ColorRgba<float>( 240 / 255f, 128 / 255f, 128 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Salmon = new ColorRgba<float>( 250 / 255f, 128 / 255f, 114 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkSalmon = new ColorRgba<float>( 233 / 255f, 150 / 255f, 122 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LightSalmon = new ColorRgba<float>( 255 / 255f, 160 / 255f, 122 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> OrangeRed = new ColorRgba<float>( 255 / 255f, 69 / 255f, 0 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Tomato = new ColorRgba<float>( 255 / 255f, 99 / 255f, 71 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkOrange = new ColorRgba<float>( 255 / 255f, 140 / 255f, 0 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Coral = new ColorRgba<float>( 255 / 255f, 127 / 255f, 80 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Orange = new ColorRgba<float>( 255 / 255f, 165 / 255f, 0 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkKhaki = new ColorRgba<float>( 189 / 255f, 183 / 255f, 107 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Gold = new ColorRgba<float>( 255 / 255f, 215 / 255f, 0 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Khaki = new ColorRgba<float>( 240 / 255f, 230 / 255f, 140 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> PeachPuff = new ColorRgba<float>( 255 / 255f, 218 / 255f, 185 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Yellow = new ColorRgba<float>( 255 / 255f, 255 / 255f, 0 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> PaleGoldenrod = new ColorRgba<float>( 238 / 255f, 232 / 255f, 170 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Moccasin = new ColorRgba<float>( 255 / 255f, 228 / 255f, 181 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> PapayaWhip = new ColorRgba<float>( 255 / 255f, 239 / 255f, 213 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LightGoldenrodYellow = new ColorRgba<float>( 250 / 255f, 250 / 255f, 210 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LemonChiffon = new ColorRgba<float>( 255 / 255f, 250 / 255f, 205 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LightYellow = new ColorRgba<float>( 255 / 255f, 255 / 255f, 224 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Maroon = new ColorRgba<float>( 128 / 255f, 0 / 255f, 0 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Brown = new ColorRgba<float>( 165 / 255f, 42 / 255f, 42 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> SaddleBrown = new ColorRgba<float>( 139 / 255f, 69 / 255f, 19 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Sienna = new ColorRgba<float>( 160 / 255f, 82 / 255f, 45 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Chocolate = new ColorRgba<float>( 210 / 255f, 105 / 255f, 30 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkGoldenrod = new ColorRgba<float>( 184 / 255f, 134 / 255f, 11 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Peru = new ColorRgba<float>( 205 / 255f, 133 / 255f, 63 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> RosyBrown = new ColorRgba<float>( 188 / 255f, 143 / 255f, 143 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Goldenrod = new ColorRgba<float>( 218 / 255f, 165 / 255f, 32 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> SandyBrown = new ColorRgba<float>( 244 / 255f, 164 / 255f, 96 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Tan = new ColorRgba<float>( 210 / 255f, 180 / 255f, 140 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Burlywood = new ColorRgba<float>( 222 / 255f, 184 / 255f, 135 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Wheat = new ColorRgba<float>( 245 / 255f, 222 / 255f, 179 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> NavajoWhite = new ColorRgba<float>( 255 / 255f, 222 / 255f, 173 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Bisque = new ColorRgba<float>( 255 / 255f, 228 / 255f, 196 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> BlanchedAlmond = new ColorRgba<float>( 255 / 255f, 235 / 255f, 205 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Cornsilk = new ColorRgba<float>( 255 / 255f, 248 / 255f, 220 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Indigo = new ColorRgba<float>( 75 / 255f, 0 / 255f, 130 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Purple = new ColorRgba<float>( 128 / 255f, 0 / 255f, 128 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkMagenta = new ColorRgba<float>( 139 / 255f, 0 / 255f, 139 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkViolet = new ColorRgba<float>( 148 / 255f, 0 / 255f, 211 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkSlateBlue = new ColorRgba<float>( 72 / 255f, 61 / 255f, 139 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> BlueViolet = new ColorRgba<float>( 138 / 255f, 43 / 255f, 226 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkOrchid = new ColorRgba<float>( 153 / 255f, 50 / 255f, 204 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Fuchsia = new ColorRgba<float>( 255 / 255f, 0 / 255f, 255 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Magenta = new ColorRgba<float>( 255 / 255f, 0 / 255f, 255 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> SlateBlue = new ColorRgba<float>( 106 / 255f, 90 / 255f, 205 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> MediumSlateBlue = new ColorRgba<float>( 123 / 255f, 104 / 255f, 238 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> MediumOrchid = new ColorRgba<float>( 186 / 255f, 85 / 255f, 211 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> MediumPurple = new ColorRgba<float>( 147 / 255f, 112 / 255f, 219 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Orchid = new ColorRgba<float>( 218 / 255f, 112 / 255f, 214 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Violet = new ColorRgba<float>( 238 / 255f, 130 / 255f, 238 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Plum = new ColorRgba<float>( 221 / 255f, 160 / 255f, 221 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Thistle = new ColorRgba<float>( 216 / 255f, 191 / 255f, 216 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Lavender = new ColorRgba<float>( 230 / 255f, 230 / 255f, 250 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> MidnightBlue = new ColorRgba<float>( 25 / 255f, 25 / 255f, 112 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Navy = new ColorRgba<float>( 0 / 255f, 0 / 255f, 128 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkBlue = new ColorRgba<float>( 0 / 255f, 0 / 255f, 139 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> MediumBlue = new ColorRgba<float>( 0 / 255f, 0 / 255f, 205 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Blue = new ColorRgba<float>( 0 / 255f, 0 / 255f, 255 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> RoyalBlue = new ColorRgba<float>( 65 / 255f, 105 / 255f, 225 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> SteelBlue = new ColorRgba<float>( 70 / 255f, 130 / 255f, 180 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DodgerBlue = new ColorRgba<float>( 30 / 255f, 144 / 255f, 255 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DeepSkyBlue = new ColorRgba<float>( 0 / 255f, 191 / 255f, 255 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> CornflowerBlue = new ColorRgba<float>( 100 / 255f, 149 / 255f, 237 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> SkyBlue = new ColorRgba<float>( 135 / 255f, 206 / 255f, 235 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LightSkyBlue = new ColorRgba<float>( 135 / 255f, 206 / 255f, 250 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LightSteelBlue = new ColorRgba<float>( 176 / 255f, 196 / 255f, 222 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LightBlue = new ColorRgba<float>( 173 / 255f, 216 / 255f, 230 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> PowderBlue = new ColorRgba<float>( 176 / 255f, 224 / 255f, 230 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Teal = new ColorRgba<float>( 0 / 255f, 128 / 255f, 128 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkCyan = new ColorRgba<float>( 0 / 255f, 139 / 255f, 139 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LightSeaGreen = new ColorRgba<float>( 32 / 255f, 178 / 255f, 170 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> CadetBlue = new ColorRgba<float>( 95 / 255f, 158 / 255f, 160 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkTurquoise = new ColorRgba<float>( 0 / 255f, 206 / 255f, 209 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> MediumTurquoise = new ColorRgba<float>( 72 / 255f, 209 / 255f, 204 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Turquoise = new ColorRgba<float>( 64 / 255f, 224 / 255f, 208 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Aqua = new ColorRgba<float>( 0 / 255f, 255 / 255f, 255 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Cyan = new ColorRgba<float>( 0 / 255f, 255 / 255f, 255 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Aquamarine = new ColorRgba<float>( 127 / 255f, 255 / 255f, 212 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> PaleTurquoise = new ColorRgba<float>( 175 / 255f, 238 / 255f, 238 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LightCyan = new ColorRgba<float>( 224 / 255f, 255 / 255f, 255 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkGreen = new ColorRgba<float>( 0 / 255f, 100 / 255f, 0 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Green = new ColorRgba<float>( 0 / 255f, 128 / 255f, 0 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkOliveGreen = new ColorRgba<float>( 85 / 255f, 107 / 255f, 47 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> ForestGreen = new ColorRgba<float>( 34 / 255f, 139 / 255f, 34 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> SeaGreen = new ColorRgba<float>( 46 / 255f, 139 / 255f, 87 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Olive = new ColorRgba<float>( 128 / 255f, 128 / 255f, 0 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> OliveDrab = new ColorRgba<float>( 107 / 255f, 142 / 255f, 35 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> MediumSeaGreen = new ColorRgba<float>( 60 / 255f, 179 / 255f, 113 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LimeGreen = new ColorRgba<float>( 50 / 255f, 205 / 255f, 50 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Lime = new ColorRgba<float>( 0 / 255f, 255 / 255f, 0 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> SpringGreen = new ColorRgba<float>( 0 / 255f, 255 / 255f, 127 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> MediumSpringGreen = new ColorRgba<float>( 0 / 255f, 250 / 255f, 154 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkSeaGreen = new ColorRgba<float>( 143 / 255f, 188 / 255f, 143 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> MediumAquamarine = new ColorRgba<float>( 102 / 255f, 205 / 255f, 170 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> YellowGreen = new ColorRgba<float>( 154 / 255f, 205 / 255f, 50 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LawnGreen = new ColorRgba<float>( 124 / 255f, 252 / 255f, 0 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Chartreuse = new ColorRgba<float>( 127 / 255f, 255 / 255f, 0 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LightGreen = new ColorRgba<float>( 144 / 255f, 238 / 255f, 144 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> GreenYellow = new ColorRgba<float>( 173 / 255f, 255 / 255f, 47 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> PaleGreen = new ColorRgba<float>( 152 / 255f, 251 / 255f, 152 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> MistyRose = new ColorRgba<float>( 255 / 255f, 228 / 255f, 225 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> AntiqueWhite = new ColorRgba<float>( 250 / 255f, 235 / 255f, 215 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Linen = new ColorRgba<float>( 250 / 255f, 240 / 255f, 230 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Beige = new ColorRgba<float>( 245 / 255f, 245 / 255f, 220 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> WhiteSmoke = new ColorRgba<float>( 245 / 255f, 245 / 255f, 245 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LavenderBlush = new ColorRgba<float>( 255 / 255f, 240 / 255f, 245 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> OldLace = new ColorRgba<float>( 253 / 255f, 245 / 255f, 230 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> AliceBlue = new ColorRgba<float>( 240 / 255f, 248 / 255f, 255 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Seashell = new ColorRgba<float>( 255 / 255f, 245 / 255f, 238 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> GhostWhite = new ColorRgba<float>( 248 / 255f, 248 / 255f, 255 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Honeydew = new ColorRgba<float>( 240 / 255f, 255 / 255f, 240 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> FloralWhite = new ColorRgba<float>( 255 / 255f, 250 / 255f, 240 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Azure = new ColorRgba<float>( 240 / 255f, 255 / 255f, 255 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> MintCream = new ColorRgba<float>( 245 / 255f, 255 / 255f, 250 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Snow = new ColorRgba<float>( 255 / 255f, 250 / 255f, 250 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Ivory = new ColorRgba<float>( 255 / 255f, 255 / 255f, 240 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> White = new ColorRgba<float>( 255 / 255f, 255 / 255f, 255 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Black = new ColorRgba<float>( 0 / 255f, 0 / 255f, 0 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkSlateGray = new ColorRgba<float>( 47 / 255f, 79 / 255f, 79 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DimGray = new ColorRgba<float>( 105 / 255f, 105 / 255f, 105 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> SlateGray = new ColorRgba<float>( 112 / 255f, 128 / 255f, 144 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Gray = new ColorRgba<float>( 128 / 255f, 128 / 255f, 128 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LightSlateGray = new ColorRgba<float>( 119 / 255f, 136 / 255f, 153 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> DarkGray = new ColorRgba<float>( 169 / 255f, 169 / 255f, 169 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Silver = new ColorRgba<float>( 192 / 255f, 192 / 255f, 192 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> LightGray = new ColorRgba<float>( 211 / 255f, 211 / 255f, 211 / 255f, 255 / 255f );
	public static readonly ColorRgba<float> Gainsboro = new ColorRgba<float>( 220 / 255f, 220 / 255f, 220 / 255f, 255 / 255f );

	public static readonly ColorRgba<float> Transparent = new ColorRgba<float>( 0, 0, 0, 0 );

	public static ColorRgba<byte> ToByte<T> ( this ColorRgba<T> color ) where T : IFloatingPoint<T> {
		var _255 = T.CreateChecked( 255 );
		return new() {
			R = byte.CreateTruncating( color.R * _255 ),
			G = byte.CreateTruncating( color.G * _255 ),
			B = byte.CreateTruncating( color.B * _255 ),
			A = byte.CreateTruncating( color.A * _255 )
		};
	}
	public static ColorRgba<T> ToFloat<T> ( this ColorRgba<byte> color ) where T : IFloatingPoint<T> {
		var _255 = T.CreateChecked( 255 );
		return new() {
			R = T.CreateTruncating( color.R ) / _255,
			G = T.CreateTruncating( color.G ) / _255,
			B = T.CreateTruncating( color.B ) / _255,
			A = T.CreateTruncating( color.A ) / _255
		};
	}

	public static ColorRgb<T> GetRgb<T> ( this ColorRgba<T> color ) where T : IFloatingPoint<T> {
		if ( color.A <= T.Zero ) {
			return new( T.Zero, T.Zero, T.Zero );
		}

		return new() {
			R = color.R / color.A,
			G = color.G / color.A,
			B = color.B / color.A
		};
	}
	public static ColorRgb<byte> GetRgb ( this ColorRgba<byte> color ) {
		if ( color.A <= 0 ) {
			return new( 0, 0, 0 );
		}

		return new() {
			R = (byte)((float)color.R / color.A * 255),
			G = (byte)((float)color.G / color.A * 255),
			B = (byte)((float)color.B / color.A * 255)
		};
	}

	public static ColorRgba<T> WithRgb<T> ( this ColorRgba<T> color, ColorRgb<T> value ) where T : IFloatingPoint<T> {
		if ( color.A <= T.Zero ) {
			return color;
		}
		
		return new() {
			R = value.R / color.A,
			G = value.G / color.A,
			B = value.B / color.A,
			A = color.A
		};
	}
	public static ColorRgba<byte> WithRgb ( this ColorRgba<byte> color, ColorRgb<byte> value ) {
		if ( color.A <= 0 ) {
			return color;
		}

		return new() {
			R = (byte)((float)value.R / color.A * 255),
			G = (byte)((float)value.G / color.A * 255),
			B = (byte)((float)value.B / color.A * 255),
			A = color.A
		};
	}

	public static ColorRgba<T> WithOpacity<T> ( this ColorRgba<T> color, T alpha ) where T : IFloatingPoint<T> {
		if ( color.A <= T.Zero ) {
			return color;
		}

		var mult = alpha / color.A;
		return new() {
			R = color.R * mult,
			G = color.G * mult,
			B = color.B * mult,
			A = alpha
		};
	}
	public static ColorRgba<byte> WithOpacity ( this ColorRgba<byte> color, byte alpha ) {
		if ( color.A <= 0 ) {
			return color;
		}

		var mult = (float)alpha / color.A;
		return new() {
			R = (byte)(color.R * mult),
			G = (byte)(color.G * mult),
			B = (byte)(color.B * mult),
			A = alpha
		};
	}

	public static ColorRgba<T> Interpolate<T, TTime> ( this ColorRgba<T> from, ColorRgba<T> to, TTime time ) where T : INumber<T> where TTime : INumber<TTime>, IMultiplyOperators<TTime, T, T> {
		return new() {
			R = from.R.Lerp( to.R, time ),
			G = from.G.Lerp( to.G, time ),
			B = from.B.Lerp( to.B, time ),
			A = from.A.Lerp( to.A, time )
		};
	}
}