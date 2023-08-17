using System.Numerics;

namespace Vit.Framework.Graphics;

/// <summary>
/// A color represented in the lionear RGB model with an alpha component.
/// </summary>
/// <remarks>
/// The RBG values are linear in absolute brightness. <br/>
/// See: <see href="https://blog.johnnovak.net/2016/09/21/what-every-coder-should-know-about-gamma/#gradients"/>
/// </remarks>
public struct LinearColorRgba<T> where T : INumber<T> {
	public T R;
	public T G;
	public T B;
	public T A;

	public LinearColorRgba ( T r, T g, T b, T a ) {
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public LinearColorRgb<T> Rgb {
		get => new( R, G, B );
		set {
			R = value.R;
			G = value.G;
			B = value.B;
		}
	}

	public static LinearColorRgba<T> operator / ( LinearColorRgba<T> color, T scalar ) {
		return new() {
			R = color.R / scalar,
			G = color.G / scalar,
			B = color.B / scalar,
			A = color.A
		};
	}
}

/// <inheritdoc cref="LinearColorRgba{T}"/>
public static class LinearColorRgba {
	public static readonly LinearColorRgba<float> MediumVioletRed = new LinearColorRgba<float>( 199, 21, 133, 255 ) / 255;
	public static readonly LinearColorRgba<float> DeepPink = new LinearColorRgba<float>( 255, 20, 147, 255 ) / 255;
	public static readonly LinearColorRgba<float> PaleVioletRed = new LinearColorRgba<float>( 219, 112, 147, 255 ) / 255;
	public static readonly LinearColorRgba<float> HotPink = new LinearColorRgba<float>( 255, 105, 180, 255 ) / 255;
	public static readonly LinearColorRgba<float> LightPink = new LinearColorRgba<float>( 255, 182, 193, 255 ) / 255;
	public static readonly LinearColorRgba<float> Pink = new LinearColorRgba<float>( 255, 192, 203, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkRed = new LinearColorRgba<float>( 139, 0, 0, 255 ) / 255;
	public static readonly LinearColorRgba<float> Red = new LinearColorRgba<float>( 255, 0, 0, 255 ) / 255;
	public static readonly LinearColorRgba<float> Firebrick = new LinearColorRgba<float>( 178, 34, 34, 255 ) / 255;
	public static readonly LinearColorRgba<float> Crimson = new LinearColorRgba<float>( 220, 20, 60, 255 ) / 255;
	public static readonly LinearColorRgba<float> IndianRed = new LinearColorRgba<float>( 205, 92, 92, 255 ) / 255;
	public static readonly LinearColorRgba<float> LightCoral = new LinearColorRgba<float>( 240, 128, 128, 255 ) / 255;
	public static readonly LinearColorRgba<float> Salmon = new LinearColorRgba<float>( 250, 128, 114, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkSalmon = new LinearColorRgba<float>( 233, 150, 122, 255 ) / 255;
	public static readonly LinearColorRgba<float> LightSalmon = new LinearColorRgba<float>( 255, 160, 122, 255 ) / 255;
	public static readonly LinearColorRgba<float> OrangeRed = new LinearColorRgba<float>( 255, 69, 0, 255 ) / 255;
	public static readonly LinearColorRgba<float> Tomato = new LinearColorRgba<float>( 255, 99, 71, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkOrange = new LinearColorRgba<float>( 255, 140, 0, 255 ) / 255;
	public static readonly LinearColorRgba<float> Coral = new LinearColorRgba<float>( 255, 127, 80, 255 ) / 255;
	public static readonly LinearColorRgba<float> Orange = new LinearColorRgba<float>( 255, 165, 0, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkKhaki = new LinearColorRgba<float>( 189, 183, 107, 255 ) / 255;
	public static readonly LinearColorRgba<float> Gold = new LinearColorRgba<float>( 255, 215, 0, 255 ) / 255;
	public static readonly LinearColorRgba<float> Khaki = new LinearColorRgba<float>( 240, 230, 140, 255 ) / 255;
	public static readonly LinearColorRgba<float> PeachPuff = new LinearColorRgba<float>( 255, 218, 185, 255 ) / 255;
	public static readonly LinearColorRgba<float> Yellow = new LinearColorRgba<float>( 255, 255, 0, 255 ) / 255;
	public static readonly LinearColorRgba<float> PaleGoldenrod = new LinearColorRgba<float>( 238, 232, 170, 255 ) / 255;
	public static readonly LinearColorRgba<float> Moccasin = new LinearColorRgba<float>( 255, 228, 181, 255 ) / 255;
	public static readonly LinearColorRgba<float> PapayaWhip = new LinearColorRgba<float>( 255, 239, 213, 255 ) / 255;
	public static readonly LinearColorRgba<float> LightGoldenrodYellow = new LinearColorRgba<float>( 250, 250, 210, 255 ) / 255;
	public static readonly LinearColorRgba<float> LemonChiffon = new LinearColorRgba<float>( 255, 250, 205, 255 ) / 255;
	public static readonly LinearColorRgba<float> LightYellow = new LinearColorRgba<float>( 255, 255, 224, 255 ) / 255;
	public static readonly LinearColorRgba<float> Maroon = new LinearColorRgba<float>( 128, 0, 0, 255 ) / 255;
	public static readonly LinearColorRgba<float> Brown = new LinearColorRgba<float>( 165, 42, 42, 255 ) / 255;
	public static readonly LinearColorRgba<float> SaddleBrown = new LinearColorRgba<float>( 139, 69, 19, 255 ) / 255;
	public static readonly LinearColorRgba<float> Sienna = new LinearColorRgba<float>( 160, 82, 45, 255 ) / 255;
	public static readonly LinearColorRgba<float> Chocolate = new LinearColorRgba<float>( 210, 105, 30, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkGoldenrod = new LinearColorRgba<float>( 184, 134, 11, 255 ) / 255;
	public static readonly LinearColorRgba<float> Peru = new LinearColorRgba<float>( 205, 133, 63, 255 ) / 255;
	public static readonly LinearColorRgba<float> RosyBrown = new LinearColorRgba<float>( 188, 143, 143, 255 ) / 255;
	public static readonly LinearColorRgba<float> Goldenrod = new LinearColorRgba<float>( 218, 165, 32, 255 ) / 255;
	public static readonly LinearColorRgba<float> SandyBrown = new LinearColorRgba<float>( 244, 164, 96, 255 ) / 255;
	public static readonly LinearColorRgba<float> Tan = new LinearColorRgba<float>( 210, 180, 140, 255 ) / 255;
	public static readonly LinearColorRgba<float> Burlywood = new LinearColorRgba<float>( 222, 184, 135, 255 ) / 255;
	public static readonly LinearColorRgba<float> Wheat = new LinearColorRgba<float>( 245, 222, 179, 255 ) / 255;
	public static readonly LinearColorRgba<float> NavajoWhite = new LinearColorRgba<float>( 255, 222, 173, 255 ) / 255;
	public static readonly LinearColorRgba<float> Bisque = new LinearColorRgba<float>( 255, 228, 196, 255 ) / 255;
	public static readonly LinearColorRgba<float> BlanchedAlmond = new LinearColorRgba<float>( 255, 235, 205, 255 ) / 255;
	public static readonly LinearColorRgba<float> Cornsilk = new LinearColorRgba<float>( 255, 248, 220, 255 ) / 255;
	public static readonly LinearColorRgba<float> Indigo = new LinearColorRgba<float>( 75, 0, 130, 255 ) / 255;
	public static readonly LinearColorRgba<float> Purple = new LinearColorRgba<float>( 128, 0, 128, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkMagenta = new LinearColorRgba<float>( 139, 0, 139, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkViolet = new LinearColorRgba<float>( 148, 0, 211, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkSlateBlue = new LinearColorRgba<float>( 72, 61, 139, 255 ) / 255;
	public static readonly LinearColorRgba<float> BlueViolet = new LinearColorRgba<float>( 138, 43, 226, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkOrchid = new LinearColorRgba<float>( 153, 50, 204, 255 ) / 255;
	public static readonly LinearColorRgba<float> Fuchsia = new LinearColorRgba<float>( 255, 0, 255, 255 ) / 255;
	public static readonly LinearColorRgba<float> Magenta = new LinearColorRgba<float>( 255, 0, 255, 255 ) / 255;
	public static readonly LinearColorRgba<float> SlateBlue = new LinearColorRgba<float>( 106, 90, 205, 255 ) / 255;
	public static readonly LinearColorRgba<float> MediumSlateBlue = new LinearColorRgba<float>( 123, 104, 238, 255 ) / 255;
	public static readonly LinearColorRgba<float> MediumOrchid = new LinearColorRgba<float>( 186, 85, 211, 255 ) / 255;
	public static readonly LinearColorRgba<float> MediumPurple = new LinearColorRgba<float>( 147, 112, 219, 255 ) / 255;
	public static readonly LinearColorRgba<float> Orchid = new LinearColorRgba<float>( 218, 112, 214, 255 ) / 255;
	public static readonly LinearColorRgba<float> Violet = new LinearColorRgba<float>( 238, 130, 238, 255 ) / 255;
	public static readonly LinearColorRgba<float> Plum = new LinearColorRgba<float>( 221, 160, 221, 255 ) / 255;
	public static readonly LinearColorRgba<float> Thistle = new LinearColorRgba<float>( 216, 191, 216, 255 ) / 255;
	public static readonly LinearColorRgba<float> Lavender = new LinearColorRgba<float>( 230, 230, 250, 255 ) / 255;
	public static readonly LinearColorRgba<float> MidnightBlue = new LinearColorRgba<float>( 25, 25, 112, 255 ) / 255;
	public static readonly LinearColorRgba<float> Navy = new LinearColorRgba<float>( 0, 0, 128, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkBlue = new LinearColorRgba<float>( 0, 0, 139, 255 ) / 255;
	public static readonly LinearColorRgba<float> MediumBlue = new LinearColorRgba<float>( 0, 0, 205, 255 ) / 255;
	public static readonly LinearColorRgba<float> Blue = new LinearColorRgba<float>( 0, 0, 255, 255 ) / 255;
	public static readonly LinearColorRgba<float> RoyalBlue = new LinearColorRgba<float>( 65, 105, 225, 255 ) / 255;
	public static readonly LinearColorRgba<float> SteelBlue = new LinearColorRgba<float>( 70, 130, 180, 255 ) / 255;
	public static readonly LinearColorRgba<float> DodgerBlue = new LinearColorRgba<float>( 30, 144, 255, 255 ) / 255;
	public static readonly LinearColorRgba<float> DeepSkyBlue = new LinearColorRgba<float>( 0, 191, 255, 255 ) / 255;
	public static readonly LinearColorRgba<float> CornflowerBlue = new LinearColorRgba<float>( 100, 149, 237, 255 ) / 255;
	public static readonly LinearColorRgba<float> SkyBlue = new LinearColorRgba<float>( 135, 206, 235, 255 ) / 255;
	public static readonly LinearColorRgba<float> LightSkyBlue = new LinearColorRgba<float>( 135, 206, 250, 255 ) / 255;
	public static readonly LinearColorRgba<float> LightSteelBlue = new LinearColorRgba<float>( 176, 196, 222, 255 ) / 255;
	public static readonly LinearColorRgba<float> LightBlue = new LinearColorRgba<float>( 173, 216, 230, 255 ) / 255;
	public static readonly LinearColorRgba<float> PowderBlue = new LinearColorRgba<float>( 176, 224, 230, 255 ) / 255;
	public static readonly LinearColorRgba<float> Teal = new LinearColorRgba<float>( 0, 128, 128, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkCyan = new LinearColorRgba<float>( 0, 139, 139, 255 ) / 255;
	public static readonly LinearColorRgba<float> LightSeaGreen = new LinearColorRgba<float>( 32, 178, 170, 255 ) / 255;
	public static readonly LinearColorRgba<float> CadetBlue = new LinearColorRgba<float>( 95, 158, 160, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkTurquoise = new LinearColorRgba<float>( 0, 206, 209, 255 ) / 255;
	public static readonly LinearColorRgba<float> MediumTurquoise = new LinearColorRgba<float>( 72, 209, 204, 255 ) / 255;
	public static readonly LinearColorRgba<float> Turquoise = new LinearColorRgba<float>( 64, 224, 208, 255 ) / 255;
	public static readonly LinearColorRgba<float> Aqua = new LinearColorRgba<float>( 0, 255, 255, 255 ) / 255;
	public static readonly LinearColorRgba<float> Cyan = new LinearColorRgba<float>( 0, 255, 255, 255 ) / 255;
	public static readonly LinearColorRgba<float> Aquamarine = new LinearColorRgba<float>( 127, 255, 212, 255 ) / 255;
	public static readonly LinearColorRgba<float> PaleTurquoise = new LinearColorRgba<float>( 175, 238, 238, 255 ) / 255;
	public static readonly LinearColorRgba<float> LightCyan = new LinearColorRgba<float>( 224, 255, 255, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkGreen = new LinearColorRgba<float>( 0, 100, 0, 255 ) / 255;
	public static readonly LinearColorRgba<float> Green = new LinearColorRgba<float>( 0, 128, 0, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkOliveGreen = new LinearColorRgba<float>( 85, 107, 47, 255 ) / 255;
	public static readonly LinearColorRgba<float> ForestGreen = new LinearColorRgba<float>( 34, 139, 34, 255 ) / 255;
	public static readonly LinearColorRgba<float> SeaGreen = new LinearColorRgba<float>( 46, 139, 87, 255 ) / 255;
	public static readonly LinearColorRgba<float> Olive = new LinearColorRgba<float>( 128, 128, 0, 255 ) / 255;
	public static readonly LinearColorRgba<float> OliveDrab = new LinearColorRgba<float>( 107, 142, 35, 255 ) / 255;
	public static readonly LinearColorRgba<float> MediumSeaGreen = new LinearColorRgba<float>( 60, 179, 113, 255 ) / 255;
	public static readonly LinearColorRgba<float> LimeGreen = new LinearColorRgba<float>( 50, 205, 50, 255 ) / 255;
	public static readonly LinearColorRgba<float> Lime = new LinearColorRgba<float>( 0, 255, 0, 255 ) / 255;
	public static readonly LinearColorRgba<float> SpringGreen = new LinearColorRgba<float>( 0, 255, 127, 255 ) / 255;
	public static readonly LinearColorRgba<float> MediumSpringGreen = new LinearColorRgba<float>( 0, 250, 154, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkSeaGreen = new LinearColorRgba<float>( 143, 188, 143, 255 ) / 255;
	public static readonly LinearColorRgba<float> MediumAquamarine = new LinearColorRgba<float>( 102, 205, 170, 255 ) / 255;
	public static readonly LinearColorRgba<float> YellowGreen = new LinearColorRgba<float>( 154, 205, 50, 255 ) / 255;
	public static readonly LinearColorRgba<float> LawnGreen = new LinearColorRgba<float>( 124, 252, 0, 255 ) / 255;
	public static readonly LinearColorRgba<float> Chartreuse = new LinearColorRgba<float>( 127, 255, 0, 255 ) / 255;
	public static readonly LinearColorRgba<float> LightGreen = new LinearColorRgba<float>( 144, 238, 144, 255 ) / 255;
	public static readonly LinearColorRgba<float> GreenYellow = new LinearColorRgba<float>( 173, 255, 47, 255 ) / 255;
	public static readonly LinearColorRgba<float> PaleGreen = new LinearColorRgba<float>( 152, 251, 152, 255 ) / 255;
	public static readonly LinearColorRgba<float> MistyRose = new LinearColorRgba<float>( 255, 228, 225, 255 ) / 255;
	public static readonly LinearColorRgba<float> AntiqueWhite = new LinearColorRgba<float>( 250, 235, 215, 255 ) / 255;
	public static readonly LinearColorRgba<float> Linen = new LinearColorRgba<float>( 250, 240, 230, 255 ) / 255;
	public static readonly LinearColorRgba<float> Beige = new LinearColorRgba<float>( 245, 245, 220, 255 ) / 255;
	public static readonly LinearColorRgba<float> WhiteSmoke = new LinearColorRgba<float>( 245, 245, 245, 255 ) / 255;
	public static readonly LinearColorRgba<float> LavenderBlush = new LinearColorRgba<float>( 255, 240, 245, 255 ) / 255;
	public static readonly LinearColorRgba<float> OldLace = new LinearColorRgba<float>( 253, 245, 230, 255 ) / 255;
	public static readonly LinearColorRgba<float> AliceBlue = new LinearColorRgba<float>( 240, 248, 255, 255 ) / 255;
	public static readonly LinearColorRgba<float> Seashell = new LinearColorRgba<float>( 255, 245, 238, 255 ) / 255;
	public static readonly LinearColorRgba<float> GhostWhite = new LinearColorRgba<float>( 248, 248, 255, 255 ) / 255;
	public static readonly LinearColorRgba<float> Honeydew = new LinearColorRgba<float>( 240, 255, 240, 255 ) / 255;
	public static readonly LinearColorRgba<float> FloralWhite = new LinearColorRgba<float>( 255, 250, 240, 255 ) / 255;
	public static readonly LinearColorRgba<float> Azure = new LinearColorRgba<float>( 240, 255, 255, 255 ) / 255;
	public static readonly LinearColorRgba<float> MintCream = new LinearColorRgba<float>( 245, 255, 250, 255 ) / 255;
	public static readonly LinearColorRgba<float> Snow = new LinearColorRgba<float>( 255, 250, 250, 255 ) / 255;
	public static readonly LinearColorRgba<float> Ivory = new LinearColorRgba<float>( 255, 255, 240, 255 ) / 255;
	public static readonly LinearColorRgba<float> White = new LinearColorRgba<float>( 255, 255, 255, 255 ) / 255;
	public static readonly LinearColorRgba<float> Black = new LinearColorRgba<float>( 0, 0, 0, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkSlateGray = new LinearColorRgba<float>( 47, 79, 79, 255 ) / 255;
	public static readonly LinearColorRgba<float> DimGray = new LinearColorRgba<float>( 105, 105, 105, 255 ) / 255;
	public static readonly LinearColorRgba<float> SlateGray = new LinearColorRgba<float>( 112, 128, 144, 255 ) / 255;
	public static readonly LinearColorRgba<float> Gray = new LinearColorRgba<float>( 128, 128, 128, 255 ) / 255;
	public static readonly LinearColorRgba<float> LightSlateGray = new LinearColorRgba<float>( 119, 136, 153, 255 ) / 255;
	public static readonly LinearColorRgba<float> DarkGray = new LinearColorRgba<float>( 169, 169, 169, 255 ) / 255;
	public static readonly LinearColorRgba<float> Silver = new LinearColorRgba<float>( 192, 192, 192, 255 ) / 255;
	public static readonly LinearColorRgba<float> LightGray = new LinearColorRgba<float>( 211, 211, 211, 255 ) / 255;
	public static readonly LinearColorRgba<float> Gainsboro = new LinearColorRgba<float>( 220, 220, 220, 255 ) / 255;
}