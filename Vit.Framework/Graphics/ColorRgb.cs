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
	public static readonly ColorRgb<float> MediumVioletRed = ColorRgba.MediumVioletRed.Rgb;
	public static readonly ColorRgb<float> DeepPink = ColorRgba.DeepPink.Rgb;
	public static readonly ColorRgb<float> PaleVioletRed = ColorRgba.PaleVioletRed.Rgb;
	public static readonly ColorRgb<float> HotPink = ColorRgba.HotPink.Rgb;
	public static readonly ColorRgb<float> LightPink = ColorRgba.LightPink.Rgb;
	public static readonly ColorRgb<float> Pink = ColorRgba.Pink.Rgb;
	public static readonly ColorRgb<float> DarkRed = ColorRgba.DarkRed.Rgb;
	public static readonly ColorRgb<float> Red = ColorRgba.Red.Rgb;
	public static readonly ColorRgb<float> Firebrick = ColorRgba.Firebrick.Rgb;
	public static readonly ColorRgb<float> Crimson = ColorRgba.Crimson.Rgb;
	public static readonly ColorRgb<float> IndianRed = ColorRgba.IndianRed.Rgb;
	public static readonly ColorRgb<float> LightCoral = ColorRgba.LightCoral.Rgb;
	public static readonly ColorRgb<float> Salmon = ColorRgba.Salmon.Rgb;
	public static readonly ColorRgb<float> DarkSalmon = ColorRgba.DarkSalmon.Rgb;
	public static readonly ColorRgb<float> LightSalmon = ColorRgba.LightSalmon.Rgb;
	public static readonly ColorRgb<float> OrangeRed = ColorRgba.OrangeRed.Rgb;
	public static readonly ColorRgb<float> Tomato = ColorRgba.Tomato.Rgb;
	public static readonly ColorRgb<float> DarkOrange = ColorRgba.DarkOrange.Rgb;
	public static readonly ColorRgb<float> Coral = ColorRgba.Coral.Rgb;
	public static readonly ColorRgb<float> Orange = ColorRgba.Orange.Rgb;
	public static readonly ColorRgb<float> DarkKhaki = ColorRgba.DarkKhaki.Rgb;
	public static readonly ColorRgb<float> Gold = ColorRgba.Gold.Rgb;
	public static readonly ColorRgb<float> Khaki = ColorRgba.Khaki.Rgb;
	public static readonly ColorRgb<float> PeachPuff = ColorRgba.PeachPuff.Rgb;
	public static readonly ColorRgb<float> Yellow = ColorRgba.Yellow.Rgb;
	public static readonly ColorRgb<float> PaleGoldenrod = ColorRgba.PaleGoldenrod.Rgb;
	public static readonly ColorRgb<float> Moccasin = ColorRgba.Moccasin.Rgb;
	public static readonly ColorRgb<float> PapayaWhip = ColorRgba.PapayaWhip.Rgb;
	public static readonly ColorRgb<float> LightGoldenrodYellow = ColorRgba.LightGoldenrodYellow.Rgb;
	public static readonly ColorRgb<float> LemonChiffon = ColorRgba.LemonChiffon.Rgb;
	public static readonly ColorRgb<float> LightYellow = ColorRgba.LightYellow.Rgb;
	public static readonly ColorRgb<float> Maroon = ColorRgba.Maroon.Rgb;
	public static readonly ColorRgb<float> Brown = ColorRgba.Brown.Rgb;
	public static readonly ColorRgb<float> SaddleBrown = ColorRgba.SaddleBrown.Rgb;
	public static readonly ColorRgb<float> Sienna = ColorRgba.Sienna.Rgb;
	public static readonly ColorRgb<float> Chocolate = ColorRgba.Chocolate.Rgb;
	public static readonly ColorRgb<float> DarkGoldenrod = ColorRgba.DarkGoldenrod.Rgb;
	public static readonly ColorRgb<float> Peru = ColorRgba.Peru.Rgb;
	public static readonly ColorRgb<float> RosyBrown = ColorRgba.RosyBrown.Rgb;
	public static readonly ColorRgb<float> Goldenrod = ColorRgba.Goldenrod.Rgb;
	public static readonly ColorRgb<float> SandyBrown = ColorRgba.SandyBrown.Rgb;
	public static readonly ColorRgb<float> Tan = ColorRgba.Tan.Rgb;
	public static readonly ColorRgb<float> Burlywood = ColorRgba.Burlywood.Rgb;
	public static readonly ColorRgb<float> Wheat = ColorRgba.Wheat.Rgb;
	public static readonly ColorRgb<float> NavajoWhite = ColorRgba.NavajoWhite.Rgb;
	public static readonly ColorRgb<float> Bisque = ColorRgba.Bisque.Rgb;
	public static readonly ColorRgb<float> BlanchedAlmond = ColorRgba.BlanchedAlmond.Rgb;
	public static readonly ColorRgb<float> Cornsilk = ColorRgba.Cornsilk.Rgb;
	public static readonly ColorRgb<float> Indigo = ColorRgba.Indigo.Rgb;
	public static readonly ColorRgb<float> Purple = ColorRgba.Purple.Rgb;
	public static readonly ColorRgb<float> DarkMagenta = ColorRgba.DarkMagenta.Rgb;
	public static readonly ColorRgb<float> DarkViolet = ColorRgba.DarkViolet.Rgb;
	public static readonly ColorRgb<float> DarkSlateBlue = ColorRgba.DarkSlateBlue.Rgb;
	public static readonly ColorRgb<float> BlueViolet = ColorRgba.BlueViolet.Rgb;
	public static readonly ColorRgb<float> DarkOrchid = ColorRgba.DarkOrchid.Rgb;
	public static readonly ColorRgb<float> Fuchsia = ColorRgba.Fuchsia.Rgb;
	public static readonly ColorRgb<float> Magenta = ColorRgba.Magenta.Rgb;
	public static readonly ColorRgb<float> SlateBlue = ColorRgba.SlateBlue.Rgb;
	public static readonly ColorRgb<float> MediumSlateBlue = ColorRgba.MediumSlateBlue.Rgb;
	public static readonly ColorRgb<float> MediumOrchid = ColorRgba.MediumOrchid.Rgb;
	public static readonly ColorRgb<float> MediumPurple = ColorRgba.MediumPurple.Rgb;
	public static readonly ColorRgb<float> Orchid = ColorRgba.Orchid.Rgb;
	public static readonly ColorRgb<float> Violet = ColorRgba.Violet.Rgb;
	public static readonly ColorRgb<float> Plum = ColorRgba.Plum.Rgb;
	public static readonly ColorRgb<float> Thistle = ColorRgba.Thistle.Rgb;
	public static readonly ColorRgb<float> Lavender = ColorRgba.Lavender.Rgb;
	public static readonly ColorRgb<float> MidnightBlue = ColorRgba.MidnightBlue.Rgb;
	public static readonly ColorRgb<float> Navy = ColorRgba.Navy.Rgb;
	public static readonly ColorRgb<float> DarkBlue = ColorRgba.DarkBlue.Rgb;
	public static readonly ColorRgb<float> MediumBlue = ColorRgba.MediumBlue.Rgb;
	public static readonly ColorRgb<float> Blue = ColorRgba.Blue.Rgb;
	public static readonly ColorRgb<float> RoyalBlue = ColorRgba.RoyalBlue.Rgb;
	public static readonly ColorRgb<float> SteelBlue = ColorRgba.SteelBlue.Rgb;
	public static readonly ColorRgb<float> DodgerBlue = ColorRgba.DodgerBlue.Rgb;
	public static readonly ColorRgb<float> DeepSkyBlue = ColorRgba.DeepSkyBlue.Rgb;
	public static readonly ColorRgb<float> CornflowerBlue = ColorRgba.CornflowerBlue.Rgb;
	public static readonly ColorRgb<float> SkyBlue = ColorRgba.SkyBlue.Rgb;
	public static readonly ColorRgb<float> LightSkyBlue = ColorRgba.LightSkyBlue.Rgb;
	public static readonly ColorRgb<float> LightSteelBlue = ColorRgba.LightSteelBlue.Rgb;
	public static readonly ColorRgb<float> LightBlue = ColorRgba.LightBlue.Rgb;
	public static readonly ColorRgb<float> PowderBlue = ColorRgba.PowderBlue.Rgb;
	public static readonly ColorRgb<float> Teal = ColorRgba.Teal.Rgb;
	public static readonly ColorRgb<float> DarkCyan = ColorRgba.DarkCyan.Rgb;
	public static readonly ColorRgb<float> LightSeaGreen = ColorRgba.LightSeaGreen.Rgb;
	public static readonly ColorRgb<float> CadetBlue = ColorRgba.CadetBlue.Rgb;
	public static readonly ColorRgb<float> DarkTurquoise = ColorRgba.DarkTurquoise.Rgb;
	public static readonly ColorRgb<float> MediumTurquoise = ColorRgba.MediumTurquoise.Rgb;
	public static readonly ColorRgb<float> Turquoise = ColorRgba.Turquoise.Rgb;
	public static readonly ColorRgb<float> Aqua = ColorRgba.Aqua.Rgb;
	public static readonly ColorRgb<float> Cyan = ColorRgba.Cyan.Rgb;
	public static readonly ColorRgb<float> Aquamarine = ColorRgba.Aquamarine.Rgb;
	public static readonly ColorRgb<float> PaleTurquoise = ColorRgba.PaleTurquoise.Rgb;
	public static readonly ColorRgb<float> LightCyan = ColorRgba.LightCyan.Rgb;
	public static readonly ColorRgb<float> DarkGreen = ColorRgba.DarkGreen.Rgb;
	public static readonly ColorRgb<float> Green = ColorRgba.Green.Rgb;
	public static readonly ColorRgb<float> DarkOliveGreen = ColorRgba.DarkOliveGreen.Rgb;
	public static readonly ColorRgb<float> ForestGreen = ColorRgba.ForestGreen.Rgb;
	public static readonly ColorRgb<float> SeaGreen = ColorRgba.SeaGreen.Rgb;
	public static readonly ColorRgb<float> Olive = ColorRgba.Olive.Rgb;
	public static readonly ColorRgb<float> OliveDrab = ColorRgba.OliveDrab.Rgb;
	public static readonly ColorRgb<float> MediumSeaGreen = ColorRgba.MediumSeaGreen.Rgb;
	public static readonly ColorRgb<float> LimeGreen = ColorRgba.LimeGreen.Rgb;
	public static readonly ColorRgb<float> Lime = ColorRgba.Lime.Rgb;
	public static readonly ColorRgb<float> SpringGreen = ColorRgba.SpringGreen.Rgb;
	public static readonly ColorRgb<float> MediumSpringGreen = ColorRgba.MediumSpringGreen.Rgb;
	public static readonly ColorRgb<float> DarkSeaGreen = ColorRgba.DarkSeaGreen.Rgb;
	public static readonly ColorRgb<float> MediumAquamarine = ColorRgba.MediumAquamarine.Rgb;
	public static readonly ColorRgb<float> YellowGreen = ColorRgba.YellowGreen.Rgb;
	public static readonly ColorRgb<float> LawnGreen = ColorRgba.LawnGreen.Rgb;
	public static readonly ColorRgb<float> Chartreuse = ColorRgba.Chartreuse.Rgb;
	public static readonly ColorRgb<float> LightGreen = ColorRgba.LightGreen.Rgb;
	public static readonly ColorRgb<float> GreenYellow = ColorRgba.GreenYellow.Rgb;
	public static readonly ColorRgb<float> PaleGreen = ColorRgba.PaleGreen.Rgb;
	public static readonly ColorRgb<float> MistyRose = ColorRgba.MistyRose.Rgb;
	public static readonly ColorRgb<float> AntiqueWhite = ColorRgba.AntiqueWhite.Rgb;
	public static readonly ColorRgb<float> Linen = ColorRgba.Linen.Rgb;
	public static readonly ColorRgb<float> Beige = ColorRgba.Beige.Rgb;
	public static readonly ColorRgb<float> WhiteSmoke = ColorRgba.WhiteSmoke.Rgb;
	public static readonly ColorRgb<float> LavenderBlush = ColorRgba.LavenderBlush.Rgb;
	public static readonly ColorRgb<float> OldLace = ColorRgba.OldLace.Rgb;
	public static readonly ColorRgb<float> AliceBlue = ColorRgba.AliceBlue.Rgb;
	public static readonly ColorRgb<float> Seashell = ColorRgba.Seashell.Rgb;
	public static readonly ColorRgb<float> GhostWhite = ColorRgba.GhostWhite.Rgb;
	public static readonly ColorRgb<float> Honeydew = ColorRgba.Honeydew.Rgb;
	public static readonly ColorRgb<float> FloralWhite = ColorRgba.FloralWhite.Rgb;
	public static readonly ColorRgb<float> Azure = ColorRgba.Azure.Rgb;
	public static readonly ColorRgb<float> MintCream = ColorRgba.MintCream.Rgb;
	public static readonly ColorRgb<float> Snow = ColorRgba.Snow.Rgb;
	public static readonly ColorRgb<float> Ivory = ColorRgba.Ivory.Rgb;
	public static readonly ColorRgb<float> White = ColorRgba.White.Rgb;
	public static readonly ColorRgb<float> Black = ColorRgba.Black.Rgb;
	public static readonly ColorRgb<float> DarkSlateGray = ColorRgba.DarkSlateGray.Rgb;
	public static readonly ColorRgb<float> DimGray = ColorRgba.DimGray.Rgb;
	public static readonly ColorRgb<float> SlateGray = ColorRgba.SlateGray.Rgb;
	public static readonly ColorRgb<float> Gray = ColorRgba.Gray.Rgb;
	public static readonly ColorRgb<float> LightSlateGray = ColorRgba.LightSlateGray.Rgb;
	public static readonly ColorRgb<float> DarkGray = ColorRgba.DarkGray.Rgb;
	public static readonly ColorRgb<float> Silver = ColorRgba.Silver.Rgb;
	public static readonly ColorRgb<float> LightGray = ColorRgba.LightGray.Rgb;
	public static readonly ColorRgb<float> Gainsboro = ColorRgba.Gainsboro.Rgb;

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