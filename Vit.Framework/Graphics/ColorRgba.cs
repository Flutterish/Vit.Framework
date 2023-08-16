﻿using System.Numerics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics;

/// <summary>
/// A color represented in the standard RGB model (sRGB) with an alpha component.
/// </summary>
/// <remarks>
/// The RBG values are linear in perceived brightness. <br/>
/// See: <see href="https://blog.johnnovak.net/2016/09/21/what-every-coder-should-know-about-gamma/#gradients"/>
/// </remarks>
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

	public ColorRgb<T> Rgb {
		get => new( R, G, B );
		set {
			R = value.R;
			G = value.G;
			B = value.B;
		}
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

/// <inheritdoc cref="ColorRgba{T}"/>
public static class ColorRgba {
	public static readonly ColorRgba<float> MediumVioletRed = (new LinearColorRgba<float>( 199, 21, 133, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DeepPink = (new LinearColorRgba<float>( 255, 20, 147, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> PaleVioletRed = (new LinearColorRgba<float>( 219, 112, 147, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> HotPink = (new LinearColorRgba<float>( 255, 105, 180, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LightPink = (new LinearColorRgba<float>( 255, 182, 193, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Pink = (new LinearColorRgba<float>( 255, 192, 203, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkRed = (new LinearColorRgba<float>( 139, 0, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Red = (new LinearColorRgba<float>( 255, 0, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Firebrick = (new LinearColorRgba<float>( 178, 34, 34, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Crimson = (new LinearColorRgba<float>( 220, 20, 60, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> IndianRed = (new LinearColorRgba<float>( 205, 92, 92, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LightCoral = (new LinearColorRgba<float>( 240, 128, 128, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Salmon = (new LinearColorRgba<float>( 250, 128, 114, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkSalmon = (new LinearColorRgba<float>( 233, 150, 122, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LightSalmon = (new LinearColorRgba<float>( 255, 160, 122, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> OrangeRed = (new LinearColorRgba<float>( 255, 69, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Tomato = (new LinearColorRgba<float>( 255, 99, 71, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkOrange = (new LinearColorRgba<float>( 255, 140, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Coral = (new LinearColorRgba<float>( 255, 127, 80, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Orange = (new LinearColorRgba<float>( 255, 165, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkKhaki = (new LinearColorRgba<float>( 189, 183, 107, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Gold = (new LinearColorRgba<float>( 255, 215, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Khaki = (new LinearColorRgba<float>( 240, 230, 140, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> PeachPuff = (new LinearColorRgba<float>( 255, 218, 185, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Yellow = (new LinearColorRgba<float>( 255, 255, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> PaleGoldenrod = (new LinearColorRgba<float>( 238, 232, 170, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Moccasin = (new LinearColorRgba<float>( 255, 228, 181, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> PapayaWhip = (new LinearColorRgba<float>( 255, 239, 213, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LightGoldenrodYellow = (new LinearColorRgba<float>( 250, 250, 210, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LemonChiffon = (new LinearColorRgba<float>( 255, 250, 205, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LightYellow = (new LinearColorRgba<float>( 255, 255, 224, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Maroon = (new LinearColorRgba<float>( 128, 0, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Brown = (new LinearColorRgba<float>( 165, 42, 42, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> SaddleBrown = (new LinearColorRgba<float>( 139, 69, 19, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Sienna = (new LinearColorRgba<float>( 160, 82, 45, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Chocolate = (new LinearColorRgba<float>( 210, 105, 30, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkGoldenrod = (new LinearColorRgba<float>( 184, 134, 11, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Peru = (new LinearColorRgba<float>( 205, 133, 63, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> RosyBrown = (new LinearColorRgba<float>( 188, 143, 143, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Goldenrod = (new LinearColorRgba<float>( 218, 165, 32, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> SandyBrown = (new LinearColorRgba<float>( 244, 164, 96, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Tan = (new LinearColorRgba<float>( 210, 180, 140, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Burlywood = (new LinearColorRgba<float>( 222, 184, 135, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Wheat = (new LinearColorRgba<float>( 245, 222, 179, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> NavajoWhite = (new LinearColorRgba<float>( 255, 222, 173, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Bisque = (new LinearColorRgba<float>( 255, 228, 196, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> BlanchedAlmond = (new LinearColorRgba<float>( 255, 235, 205, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Cornsilk = (new LinearColorRgba<float>( 255, 248, 220, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Indigo = (new LinearColorRgba<float>( 75, 0, 130, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Purple = (new LinearColorRgba<float>( 128, 0, 128, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkMagenta = (new LinearColorRgba<float>( 139, 0, 139, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkViolet = (new LinearColorRgba<float>( 148, 0, 211, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkSlateBlue = (new LinearColorRgba<float>( 72, 61, 139, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> BlueViolet = (new LinearColorRgba<float>( 138, 43, 226, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkOrchid = (new LinearColorRgba<float>( 153, 50, 204, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Fuchsia = (new LinearColorRgba<float>( 255, 0, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Magenta = (new LinearColorRgba<float>( 255, 0, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> SlateBlue = (new LinearColorRgba<float>( 106, 90, 205, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> MediumSlateBlue = (new LinearColorRgba<float>( 123, 104, 238, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> MediumOrchid = (new LinearColorRgba<float>( 186, 85, 211, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> MediumPurple = (new LinearColorRgba<float>( 147, 112, 219, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Orchid = (new LinearColorRgba<float>( 218, 112, 214, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Violet = (new LinearColorRgba<float>( 238, 130, 238, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Plum = (new LinearColorRgba<float>( 221, 160, 221, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Thistle = (new LinearColorRgba<float>( 216, 191, 216, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Lavender = (new LinearColorRgba<float>( 230, 230, 250, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> MidnightBlue = (new LinearColorRgba<float>( 25, 25, 112, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Navy = (new LinearColorRgba<float>( 0, 0, 128, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkBlue = (new LinearColorRgba<float>( 0, 0, 139, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> MediumBlue = (new LinearColorRgba<float>( 0, 0, 205, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Blue = (new LinearColorRgba<float>( 0, 0, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> RoyalBlue = (new LinearColorRgba<float>( 65, 105, 225, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> SteelBlue = (new LinearColorRgba<float>( 70, 130, 180, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DodgerBlue = (new LinearColorRgba<float>( 30, 144, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DeepSkyBlue = (new LinearColorRgba<float>( 0, 191, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> CornflowerBlue = (new LinearColorRgba<float>( 100, 149, 237, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> SkyBlue = (new LinearColorRgba<float>( 135, 206, 235, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LightSkyBlue = (new LinearColorRgba<float>( 135, 206, 250, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LightSteelBlue = (new LinearColorRgba<float>( 176, 196, 222, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LightBlue = (new LinearColorRgba<float>( 173, 216, 230, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> PowderBlue = (new LinearColorRgba<float>( 176, 224, 230, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Teal = (new LinearColorRgba<float>( 0, 128, 128, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkCyan = (new LinearColorRgba<float>( 0, 139, 139, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LightSeaGreen = (new LinearColorRgba<float>( 32, 178, 170, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> CadetBlue = (new LinearColorRgba<float>( 95, 158, 160, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkTurquoise = (new LinearColorRgba<float>( 0, 206, 209, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> MediumTurquoise = (new LinearColorRgba<float>( 72, 209, 204, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Turquoise = (new LinearColorRgba<float>( 64, 224, 208, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Aqua = (new LinearColorRgba<float>( 0, 255, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Cyan = (new LinearColorRgba<float>( 0, 255, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Aquamarine = (new LinearColorRgba<float>( 127, 255, 212, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> PaleTurquoise = (new LinearColorRgba<float>( 175, 238, 238, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LightCyan = (new LinearColorRgba<float>( 224, 255, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkGreen = (new LinearColorRgba<float>( 0, 100, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Green = (new LinearColorRgba<float>( 0, 128, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkOliveGreen = (new LinearColorRgba<float>( 85, 107, 47, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> ForestGreen = (new LinearColorRgba<float>( 34, 139, 34, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> SeaGreen = (new LinearColorRgba<float>( 46, 139, 87, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Olive = (new LinearColorRgba<float>( 128, 128, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> OliveDrab = (new LinearColorRgba<float>( 107, 142, 35, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> MediumSeaGreen = (new LinearColorRgba<float>( 60, 179, 113, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LimeGreen = (new LinearColorRgba<float>( 50, 205, 50, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Lime = (new LinearColorRgba<float>( 0, 255, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> SpringGreen = (new LinearColorRgba<float>( 0, 255, 127, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> MediumSpringGreen = (new LinearColorRgba<float>( 0, 250, 154, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkSeaGreen = (new LinearColorRgba<float>( 143, 188, 143, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> MediumAquamarine = (new LinearColorRgba<float>( 102, 205, 170, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> YellowGreen = (new LinearColorRgba<float>( 154, 205, 50, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LawnGreen = (new LinearColorRgba<float>( 124, 252, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Chartreuse = (new LinearColorRgba<float>( 127, 255, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LightGreen = (new LinearColorRgba<float>( 144, 238, 144, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> GreenYellow = (new LinearColorRgba<float>( 173, 255, 47, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> PaleGreen = (new LinearColorRgba<float>( 152, 251, 152, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> MistyRose = (new LinearColorRgba<float>( 255, 228, 225, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> AntiqueWhite = (new LinearColorRgba<float>( 250, 235, 215, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Linen = (new LinearColorRgba<float>( 250, 240, 230, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Beige = (new LinearColorRgba<float>( 245, 245, 220, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> WhiteSmoke = (new LinearColorRgba<float>( 245, 245, 245, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LavenderBlush = (new LinearColorRgba<float>( 255, 240, 245, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> OldLace = (new LinearColorRgba<float>( 253, 245, 230, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> AliceBlue = (new LinearColorRgba<float>( 240, 248, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Seashell = (new LinearColorRgba<float>( 255, 245, 238, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> GhostWhite = (new LinearColorRgba<float>( 248, 248, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Honeydew = (new LinearColorRgba<float>( 240, 255, 240, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> FloralWhite = (new LinearColorRgba<float>( 255, 250, 240, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Azure = (new LinearColorRgba<float>( 240, 255, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> MintCream = (new LinearColorRgba<float>( 245, 255, 250, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Snow = (new LinearColorRgba<float>( 255, 250, 250, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Ivory = (new LinearColorRgba<float>( 255, 255, 240, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> White = (new LinearColorRgba<float>( 255, 255, 255, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Black = (new LinearColorRgba<float>( 0, 0, 0, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkSlateGray = (new LinearColorRgba<float>( 47, 79, 79, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DimGray = (new LinearColorRgba<float>( 105, 105, 105, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> SlateGray = (new LinearColorRgba<float>( 112, 128, 144, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Gray = (new LinearColorRgba<float>( 128, 128, 128, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LightSlateGray = (new LinearColorRgba<float>( 119, 136, 153, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> DarkGray = (new LinearColorRgba<float>( 169, 169, 169, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Silver = (new LinearColorRgba<float>( 192, 192, 192, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> LightGray = (new LinearColorRgba<float>( 211, 211, 211, 255 ) / 255).ToSRGB();
	public static readonly ColorRgba<float> Gainsboro = (new LinearColorRgba<float>( 220, 220, 220, 255 ) / 255).ToSRGB();

	public static ColorRgba<byte> ToByte<T> ( this ColorRgba<T> color ) where T : IFloatingPoint<T> {
		var _255 = T.CreateChecked( 255 );
		return new() {
			R = byte.CreateTruncating(color.R * _255),
			G = byte.CreateTruncating(color.G * _255),
			B = byte.CreateTruncating(color.B * _255),
			A = byte.CreateTruncating(color.A * _255)
		};
	}

	const decimal Gamma = 2.2m;
	public static LinearColorRgba<T> ToLinear<T> ( this ColorRgba<T> color ) where T : IFloatingPointIeee754<T> {
		T gamma = T.CreateSaturating( 1 / Gamma );
		return new() {
			R = T.Pow( color.R, gamma ),
			G = T.Pow( color.G, gamma ),
			B = T.Pow( color.B, gamma ),
			A = color.A
		};
	}

	public static ColorRgba<T> ToSRGB<T> ( this LinearColorRgba<T> color ) where T : IFloatingPointIeee754<T> {
		T gamma = T.CreateSaturating( Gamma );
		return new() {
			R = T.Pow( color.R, gamma ),
			G = T.Pow( color.G, gamma ),
			B = T.Pow( color.B, gamma ),
			A = color.A
		};
	}

	public static ColorRgba<T> Interpolate<T, TTime> ( this ColorRgba<T> from, ColorRgba<T> to, TTime time ) where T : INumber<T>, IFloatingPointIeee754<T> where TTime : INumber<TTime>, IMultiplyOperators<TTime, T, T> {
		return from.ToLinear().Interpolate( to.ToLinear(), time ).ToSRGB();
	}

	public static LinearColorRgba<T> Interpolate<T, TTime> ( this LinearColorRgba<T> from, LinearColorRgba<T> to, TTime time ) where T : INumber<T> where TTime : INumber<TTime>, IMultiplyOperators<TTime, T, T> {
		return new() {
			R = from.R.Lerp( to.R, time ),
			G = from.G.Lerp( to.G, time ),
			B = from.B.Lerp( to.B, time ),
			A = from.A.Lerp( to.A, time )
		};
	}
}