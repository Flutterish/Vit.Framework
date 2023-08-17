using System.Numerics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics;

/// <summary>
/// A color represented in the standard RGB model (sRGB, gamma = 2.2) with an alpha component.
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
			return $"(sRGB) #{R:X2}{G:X2}{B:X2}{( A == _255 ? "" : $"{A:X2}" )}";
		}
		else if ( typeof(T).IsAssignableTo(typeof(IFloatingPoint<>)) ) {
			var _255 = T.CreateChecked( 255 );
			var (r, g, b, a) = (byte.CreateTruncating(R * _255),byte.CreateTruncating(G * _255), byte.CreateTruncating(B * _255), byte.CreateTruncating(A * _255));
			return $"(sRGB) #{r:X2}{g:X2}{b:X2}{( a == 255 ? "" : $"{a:X2}" )}";
		}
		else {
			return $"(sRGB) ({R}, {G}, {B}, {A})";
		}
	}
}

/// <inheritdoc cref="ColorRgba{T}"/>
public static class ColorRgba {
	public static readonly ColorRgba<float> MediumVioletRed = LinearColorRgba.MediumVioletRed.ToSRGB();
	public static readonly ColorRgba<float> DeepPink = LinearColorRgba.DeepPink.ToSRGB();
	public static readonly ColorRgba<float> PaleVioletRed = LinearColorRgba.PaleVioletRed.ToSRGB();
	public static readonly ColorRgba<float> HotPink = LinearColorRgba.HotPink.ToSRGB();
	public static readonly ColorRgba<float> LightPink = LinearColorRgba.LightPink.ToSRGB();
	public static readonly ColorRgba<float> Pink = LinearColorRgba.Pink.ToSRGB();
	public static readonly ColorRgba<float> DarkRed = LinearColorRgba.DarkRed.ToSRGB();
	public static readonly ColorRgba<float> Red = LinearColorRgba.Red.ToSRGB();
	public static readonly ColorRgba<float> Firebrick = LinearColorRgba.Firebrick.ToSRGB();
	public static readonly ColorRgba<float> Crimson = LinearColorRgba.Crimson.ToSRGB();
	public static readonly ColorRgba<float> IndianRed = LinearColorRgba.IndianRed.ToSRGB();
	public static readonly ColorRgba<float> LightCoral = LinearColorRgba.LightCoral.ToSRGB();
	public static readonly ColorRgba<float> Salmon = LinearColorRgba.Salmon.ToSRGB();
	public static readonly ColorRgba<float> DarkSalmon = LinearColorRgba.DarkSalmon.ToSRGB();
	public static readonly ColorRgba<float> LightSalmon = LinearColorRgba.LightSalmon.ToSRGB();
	public static readonly ColorRgba<float> OrangeRed = LinearColorRgba.OrangeRed.ToSRGB();
	public static readonly ColorRgba<float> Tomato = LinearColorRgba.Tomato.ToSRGB();
	public static readonly ColorRgba<float> DarkOrange = LinearColorRgba.DarkOrange.ToSRGB();
	public static readonly ColorRgba<float> Coral = LinearColorRgba.Coral.ToSRGB();
	public static readonly ColorRgba<float> Orange = LinearColorRgba.Orange.ToSRGB();
	public static readonly ColorRgba<float> DarkKhaki = LinearColorRgba.DarkKhaki.ToSRGB();
	public static readonly ColorRgba<float> Gold = LinearColorRgba.Gold.ToSRGB();
	public static readonly ColorRgba<float> Khaki = LinearColorRgba.Khaki.ToSRGB();
	public static readonly ColorRgba<float> PeachPuff = LinearColorRgba.PeachPuff.ToSRGB();
	public static readonly ColorRgba<float> Yellow = LinearColorRgba.Yellow.ToSRGB();
	public static readonly ColorRgba<float> PaleGoldenrod = LinearColorRgba.PaleGoldenrod.ToSRGB();
	public static readonly ColorRgba<float> Moccasin = LinearColorRgba.Moccasin.ToSRGB();
	public static readonly ColorRgba<float> PapayaWhip = LinearColorRgba.PapayaWhip.ToSRGB();
	public static readonly ColorRgba<float> LightGoldenrodYellow = LinearColorRgba.LightGoldenrodYellow.ToSRGB();
	public static readonly ColorRgba<float> LemonChiffon = LinearColorRgba.LemonChiffon.ToSRGB();
	public static readonly ColorRgba<float> LightYellow = LinearColorRgba.LightYellow.ToSRGB();
	public static readonly ColorRgba<float> Maroon = LinearColorRgba.Maroon.ToSRGB();
	public static readonly ColorRgba<float> Brown = LinearColorRgba.Brown.ToSRGB();
	public static readonly ColorRgba<float> SaddleBrown = LinearColorRgba.SaddleBrown.ToSRGB();
	public static readonly ColorRgba<float> Sienna = LinearColorRgba.Sienna.ToSRGB();
	public static readonly ColorRgba<float> Chocolate = LinearColorRgba.Chocolate.ToSRGB();
	public static readonly ColorRgba<float> DarkGoldenrod = LinearColorRgba.DarkGoldenrod.ToSRGB();
	public static readonly ColorRgba<float> Peru = LinearColorRgba.Peru.ToSRGB();
	public static readonly ColorRgba<float> RosyBrown = LinearColorRgba.RosyBrown.ToSRGB();
	public static readonly ColorRgba<float> Goldenrod = LinearColorRgba.Goldenrod.ToSRGB();
	public static readonly ColorRgba<float> SandyBrown = LinearColorRgba.SandyBrown.ToSRGB();
	public static readonly ColorRgba<float> Tan = LinearColorRgba.Tan.ToSRGB();
	public static readonly ColorRgba<float> Burlywood = LinearColorRgba.Burlywood.ToSRGB();
	public static readonly ColorRgba<float> Wheat = LinearColorRgba.Wheat.ToSRGB();
	public static readonly ColorRgba<float> NavajoWhite = LinearColorRgba.NavajoWhite.ToSRGB();
	public static readonly ColorRgba<float> Bisque = LinearColorRgba.Bisque.ToSRGB();
	public static readonly ColorRgba<float> BlanchedAlmond = LinearColorRgba.BlanchedAlmond.ToSRGB();
	public static readonly ColorRgba<float> Cornsilk = LinearColorRgba.Cornsilk.ToSRGB();
	public static readonly ColorRgba<float> Indigo = LinearColorRgba.Indigo.ToSRGB();
	public static readonly ColorRgba<float> Purple = LinearColorRgba.Purple.ToSRGB();
	public static readonly ColorRgba<float> DarkMagenta = LinearColorRgba.DarkMagenta.ToSRGB();
	public static readonly ColorRgba<float> DarkViolet = LinearColorRgba.DarkViolet.ToSRGB();
	public static readonly ColorRgba<float> DarkSlateBlue = LinearColorRgba.DarkSlateBlue.ToSRGB();
	public static readonly ColorRgba<float> BlueViolet = LinearColorRgba.BlueViolet.ToSRGB();
	public static readonly ColorRgba<float> DarkOrchid = LinearColorRgba.DarkOrchid.ToSRGB();
	public static readonly ColorRgba<float> Fuchsia = LinearColorRgba.Fuchsia.ToSRGB();
	public static readonly ColorRgba<float> Magenta = LinearColorRgba.Magenta.ToSRGB();
	public static readonly ColorRgba<float> SlateBlue = LinearColorRgba.SlateBlue.ToSRGB();
	public static readonly ColorRgba<float> MediumSlateBlue = LinearColorRgba.MediumSlateBlue.ToSRGB();
	public static readonly ColorRgba<float> MediumOrchid = LinearColorRgba.MediumOrchid.ToSRGB();
	public static readonly ColorRgba<float> MediumPurple = LinearColorRgba.MediumPurple.ToSRGB();
	public static readonly ColorRgba<float> Orchid = LinearColorRgba.Orchid.ToSRGB();
	public static readonly ColorRgba<float> Violet = LinearColorRgba.Violet.ToSRGB();
	public static readonly ColorRgba<float> Plum = LinearColorRgba.Plum.ToSRGB();
	public static readonly ColorRgba<float> Thistle = LinearColorRgba.Thistle.ToSRGB();
	public static readonly ColorRgba<float> Lavender = LinearColorRgba.Lavender.ToSRGB();
	public static readonly ColorRgba<float> MidnightBlue = LinearColorRgba.MidnightBlue.ToSRGB();
	public static readonly ColorRgba<float> Navy = LinearColorRgba.Navy.ToSRGB();
	public static readonly ColorRgba<float> DarkBlue = LinearColorRgba.DarkBlue.ToSRGB();
	public static readonly ColorRgba<float> MediumBlue = LinearColorRgba.MediumBlue.ToSRGB();
	public static readonly ColorRgba<float> Blue = LinearColorRgba.Blue.ToSRGB();
	public static readonly ColorRgba<float> RoyalBlue = LinearColorRgba.RoyalBlue.ToSRGB();
	public static readonly ColorRgba<float> SteelBlue = LinearColorRgba.SteelBlue.ToSRGB();
	public static readonly ColorRgba<float> DodgerBlue = LinearColorRgba.DodgerBlue.ToSRGB();
	public static readonly ColorRgba<float> DeepSkyBlue = LinearColorRgba.DeepSkyBlue.ToSRGB();
	public static readonly ColorRgba<float> CornflowerBlue = LinearColorRgba.CornflowerBlue.ToSRGB();
	public static readonly ColorRgba<float> SkyBlue = LinearColorRgba.SkyBlue.ToSRGB();
	public static readonly ColorRgba<float> LightSkyBlue = LinearColorRgba.LightSkyBlue.ToSRGB();
	public static readonly ColorRgba<float> LightSteelBlue = LinearColorRgba.LightSteelBlue.ToSRGB();
	public static readonly ColorRgba<float> LightBlue = LinearColorRgba.LightBlue.ToSRGB();
	public static readonly ColorRgba<float> PowderBlue = LinearColorRgba.PowderBlue.ToSRGB();
	public static readonly ColorRgba<float> Teal = LinearColorRgba.Teal.ToSRGB();
	public static readonly ColorRgba<float> DarkCyan = LinearColorRgba.DarkCyan.ToSRGB();
	public static readonly ColorRgba<float> LightSeaGreen = LinearColorRgba.LightSeaGreen.ToSRGB();
	public static readonly ColorRgba<float> CadetBlue = LinearColorRgba.CadetBlue.ToSRGB();
	public static readonly ColorRgba<float> DarkTurquoise = LinearColorRgba.DarkTurquoise.ToSRGB();
	public static readonly ColorRgba<float> MediumTurquoise = LinearColorRgba.MediumTurquoise.ToSRGB();
	public static readonly ColorRgba<float> Turquoise = LinearColorRgba.Turquoise.ToSRGB();
	public static readonly ColorRgba<float> Aqua = LinearColorRgba.Aqua.ToSRGB();
	public static readonly ColorRgba<float> Cyan = LinearColorRgba.Cyan.ToSRGB();
	public static readonly ColorRgba<float> Aquamarine = LinearColorRgba.Aquamarine.ToSRGB();
	public static readonly ColorRgba<float> PaleTurquoise = LinearColorRgba.PaleTurquoise.ToSRGB();
	public static readonly ColorRgba<float> LightCyan = LinearColorRgba.LightCyan.ToSRGB();
	public static readonly ColorRgba<float> DarkGreen = LinearColorRgba.DarkGreen.ToSRGB();
	public static readonly ColorRgba<float> Green = LinearColorRgba.Green.ToSRGB();
	public static readonly ColorRgba<float> DarkOliveGreen = LinearColorRgba.DarkOliveGreen.ToSRGB();
	public static readonly ColorRgba<float> ForestGreen = LinearColorRgba.ForestGreen.ToSRGB();
	public static readonly ColorRgba<float> SeaGreen = LinearColorRgba.SeaGreen.ToSRGB();
	public static readonly ColorRgba<float> Olive = LinearColorRgba.Olive.ToSRGB();
	public static readonly ColorRgba<float> OliveDrab = LinearColorRgba.OliveDrab.ToSRGB();
	public static readonly ColorRgba<float> MediumSeaGreen = LinearColorRgba.MediumSeaGreen.ToSRGB();
	public static readonly ColorRgba<float> LimeGreen = LinearColorRgba.LimeGreen.ToSRGB();
	public static readonly ColorRgba<float> Lime = LinearColorRgba.Lime.ToSRGB();
	public static readonly ColorRgba<float> SpringGreen = LinearColorRgba.SpringGreen.ToSRGB();
	public static readonly ColorRgba<float> MediumSpringGreen = LinearColorRgba.MediumSpringGreen.ToSRGB();
	public static readonly ColorRgba<float> DarkSeaGreen = LinearColorRgba.DarkSeaGreen.ToSRGB();
	public static readonly ColorRgba<float> MediumAquamarine = LinearColorRgba.MediumAquamarine.ToSRGB();
	public static readonly ColorRgba<float> YellowGreen = LinearColorRgba.YellowGreen.ToSRGB();
	public static readonly ColorRgba<float> LawnGreen = LinearColorRgba.LawnGreen.ToSRGB();
	public static readonly ColorRgba<float> Chartreuse = LinearColorRgba.Chartreuse.ToSRGB();
	public static readonly ColorRgba<float> LightGreen = LinearColorRgba.LightGreen.ToSRGB();
	public static readonly ColorRgba<float> GreenYellow = LinearColorRgba.GreenYellow.ToSRGB();
	public static readonly ColorRgba<float> PaleGreen = LinearColorRgba.PaleGreen.ToSRGB();
	public static readonly ColorRgba<float> MistyRose = LinearColorRgba.MistyRose.ToSRGB();
	public static readonly ColorRgba<float> AntiqueWhite = LinearColorRgba.AntiqueWhite.ToSRGB();
	public static readonly ColorRgba<float> Linen = LinearColorRgba.Linen.ToSRGB();
	public static readonly ColorRgba<float> Beige = LinearColorRgba.Beige.ToSRGB();
	public static readonly ColorRgba<float> WhiteSmoke = LinearColorRgba.WhiteSmoke.ToSRGB();
	public static readonly ColorRgba<float> LavenderBlush = LinearColorRgba.LavenderBlush.ToSRGB();
	public static readonly ColorRgba<float> OldLace = LinearColorRgba.OldLace.ToSRGB();
	public static readonly ColorRgba<float> AliceBlue = LinearColorRgba.AliceBlue.ToSRGB();
	public static readonly ColorRgba<float> Seashell = LinearColorRgba.Seashell.ToSRGB();
	public static readonly ColorRgba<float> GhostWhite = LinearColorRgba.GhostWhite.ToSRGB();
	public static readonly ColorRgba<float> Honeydew = LinearColorRgba.Honeydew.ToSRGB();
	public static readonly ColorRgba<float> FloralWhite = LinearColorRgba.FloralWhite.ToSRGB();
	public static readonly ColorRgba<float> Azure = LinearColorRgba.Azure.ToSRGB();
	public static readonly ColorRgba<float> MintCream = LinearColorRgba.MintCream.ToSRGB();
	public static readonly ColorRgba<float> Snow = LinearColorRgba.Snow.ToSRGB();
	public static readonly ColorRgba<float> Ivory = LinearColorRgba.Ivory.ToSRGB();
	public static readonly ColorRgba<float> White = LinearColorRgba.White.ToSRGB();
	public static readonly ColorRgba<float> Black = LinearColorRgba.Black.ToSRGB();
	public static readonly ColorRgba<float> DarkSlateGray = LinearColorRgba.DarkSlateGray.ToSRGB();
	public static readonly ColorRgba<float> DimGray = LinearColorRgba.DimGray.ToSRGB();
	public static readonly ColorRgba<float> SlateGray = LinearColorRgba.SlateGray.ToSRGB();
	public static readonly ColorRgba<float> Gray = LinearColorRgba.Gray.ToSRGB();
	public static readonly ColorRgba<float> LightSlateGray = LinearColorRgba.LightSlateGray.ToSRGB();
	public static readonly ColorRgba<float> DarkGray = LinearColorRgba.DarkGray.ToSRGB();
	public static readonly ColorRgba<float> Silver = LinearColorRgba.Silver.ToSRGB();
	public static readonly ColorRgba<float> LightGray = LinearColorRgba.LightGray.ToSRGB();
	public static readonly ColorRgba<float> Gainsboro = LinearColorRgba.Gainsboro.ToSRGB();

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