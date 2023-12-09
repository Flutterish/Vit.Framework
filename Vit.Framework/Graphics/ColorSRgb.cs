using System.Numerics;
using System.Runtime.InteropServices;

namespace Vit.Framework.Graphics;

/// <summary>
/// A color represented in the standard RGB model (sRGB, gamma = 2.2).
/// </summary>
/// <remarks>
/// Note that this type should not be used with floationg-point numbers for storage, as they negate the benefits of this format.<br/>
/// The RBG values are linear in perceived brightness. <br/>
/// See: <see href="https://blog.johnnovak.net/2016/09/21/what-every-coder-should-know-about-gamma/#gradients"/>
/// </remarks>
public struct ColorSRgb<T> : IUnlabelledColor<T>, IEqualityOperators<ColorSRgb<T>, ColorSRgb<T>, bool> where T : INumber<T> {
	public ReadOnlySpan<T> AsSpan () => MemoryMarshal.CreateReadOnlySpan( ref R, 3 );
	public T R;
	public T G;
	public T B;

	public ColorSRgb ( T r, T g, T b ) {
		R = r;
		G = g;
		B = b;
	}

	public static bool operator == ( ColorSRgb<T> left, ColorSRgb<T> right ) {
		return left.R == right.R
			&& left.G == right.G
			&& left.B == right.B;
	}

	public static bool operator != ( ColorSRgb<T> left, ColorSRgb<T> right ) {
		return left.R != right.R
			|| left.G != right.G
			|| left.B != right.B;
	}

	public override string ToString () {
		if ( typeof(T) == typeof(byte) ) {
			return $"(sRGB) #{R:X2}{G:X2}{B:X2}";
		}
		else if ( typeof(T).IsAssignableTo(typeof(IFloatingPoint<>)) ) {
			var _255 = T.CreateChecked( 255 );
			var (r, g, b) = (byte.CreateTruncating(R * _255),byte.CreateTruncating(G * _255), byte.CreateTruncating(B * _255));
			return $"(sRGB) #{r:X2}{g:X2}{b:X2}";
		}
		else {
			return $"(sRGB) ({R}, {G}, {B})";
		}
	}
}

/// <inheritdoc cref="ColorSRgb{T}"/>
public static class ColorSRgb {
	public static readonly ColorSRgb<float> MediumVioletRed = ColorRgba.MediumVioletRed.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DeepPink = ColorRgba.DeepPink.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> PaleVioletRed = ColorRgba.PaleVioletRed.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> HotPink = ColorRgba.HotPink.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LightPink = ColorRgba.LightPink.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Pink = ColorRgba.Pink.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkRed = ColorRgba.DarkRed.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Red = ColorRgba.Red.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Firebrick = ColorRgba.Firebrick.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Crimson = ColorRgba.Crimson.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> IndianRed = ColorRgba.IndianRed.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LightCoral = ColorRgba.LightCoral.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Salmon = ColorRgba.Salmon.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkSalmon = ColorRgba.DarkSalmon.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LightSalmon = ColorRgba.LightSalmon.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> OrangeRed = ColorRgba.OrangeRed.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Tomato = ColorRgba.Tomato.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkOrange = ColorRgba.DarkOrange.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Coral = ColorRgba.Coral.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Orange = ColorRgba.Orange.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkKhaki = ColorRgba.DarkKhaki.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Gold = ColorRgba.Gold.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Khaki = ColorRgba.Khaki.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> PeachPuff = ColorRgba.PeachPuff.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Yellow = ColorRgba.Yellow.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> PaleGoldenrod = ColorRgba.PaleGoldenrod.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Moccasin = ColorRgba.Moccasin.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> PapayaWhip = ColorRgba.PapayaWhip.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LightGoldenrodYellow = ColorRgba.LightGoldenrodYellow.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LemonChiffon = ColorRgba.LemonChiffon.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LightYellow = ColorRgba.LightYellow.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Maroon = ColorRgba.Maroon.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Brown = ColorRgba.Brown.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> SaddleBrown = ColorRgba.SaddleBrown.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Sienna = ColorRgba.Sienna.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Chocolate = ColorRgba.Chocolate.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkGoldenrod = ColorRgba.DarkGoldenrod.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Peru = ColorRgba.Peru.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> RosyBrown = ColorRgba.RosyBrown.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Goldenrod = ColorRgba.Goldenrod.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> SandyBrown = ColorRgba.SandyBrown.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Tan = ColorRgba.Tan.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Burlywood = ColorRgba.Burlywood.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Wheat = ColorRgba.Wheat.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> NavajoWhite = ColorRgba.NavajoWhite.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Bisque = ColorRgba.Bisque.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> BlanchedAlmond = ColorRgba.BlanchedAlmond.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Cornsilk = ColorRgba.Cornsilk.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Indigo = ColorRgba.Indigo.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Purple = ColorRgba.Purple.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkMagenta = ColorRgba.DarkMagenta.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkViolet = ColorRgba.DarkViolet.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkSlateBlue = ColorRgba.DarkSlateBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> BlueViolet = ColorRgba.BlueViolet.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkOrchid = ColorRgba.DarkOrchid.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Fuchsia = ColorRgba.Fuchsia.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Magenta = ColorRgba.Magenta.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> SlateBlue = ColorRgba.SlateBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> MediumSlateBlue = ColorRgba.MediumSlateBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> MediumOrchid = ColorRgba.MediumOrchid.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> MediumPurple = ColorRgba.MediumPurple.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Orchid = ColorRgba.Orchid.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Violet = ColorRgba.Violet.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Plum = ColorRgba.Plum.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Thistle = ColorRgba.Thistle.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Lavender = ColorRgba.Lavender.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> MidnightBlue = ColorRgba.MidnightBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Navy = ColorRgba.Navy.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkBlue = ColorRgba.DarkBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> MediumBlue = ColorRgba.MediumBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Blue = ColorRgba.Blue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> RoyalBlue = ColorRgba.RoyalBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> SteelBlue = ColorRgba.SteelBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DodgerBlue = ColorRgba.DodgerBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DeepSkyBlue = ColorRgba.DeepSkyBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> CornflowerBlue = ColorRgba.CornflowerBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> SkyBlue = ColorRgba.SkyBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LightSkyBlue = ColorRgba.LightSkyBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LightSteelBlue = ColorRgba.LightSteelBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LightBlue = ColorRgba.LightBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> PowderBlue = ColorRgba.PowderBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Teal = ColorRgba.Teal.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkCyan = ColorRgba.DarkCyan.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LightSeaGreen = ColorRgba.LightSeaGreen.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> CadetBlue = ColorRgba.CadetBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkTurquoise = ColorRgba.DarkTurquoise.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> MediumTurquoise = ColorRgba.MediumTurquoise.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Turquoise = ColorRgba.Turquoise.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Aqua = ColorRgba.Aqua.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Cyan = ColorRgba.Cyan.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Aquamarine = ColorRgba.Aquamarine.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> PaleTurquoise = ColorRgba.PaleTurquoise.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LightCyan = ColorRgba.LightCyan.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkGreen = ColorRgba.DarkGreen.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Green = ColorRgba.Green.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkOliveGreen = ColorRgba.DarkOliveGreen.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> ForestGreen = ColorRgba.ForestGreen.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> SeaGreen = ColorRgba.SeaGreen.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Olive = ColorRgba.Olive.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> OliveDrab = ColorRgba.OliveDrab.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> MediumSeaGreen = ColorRgba.MediumSeaGreen.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LimeGreen = ColorRgba.LimeGreen.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Lime = ColorRgba.Lime.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> SpringGreen = ColorRgba.SpringGreen.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> MediumSpringGreen = ColorRgba.MediumSpringGreen.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkSeaGreen = ColorRgba.DarkSeaGreen.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> MediumAquamarine = ColorRgba.MediumAquamarine.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> YellowGreen = ColorRgba.YellowGreen.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LawnGreen = ColorRgba.LawnGreen.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Chartreuse = ColorRgba.Chartreuse.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LightGreen = ColorRgba.LightGreen.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> GreenYellow = ColorRgba.GreenYellow.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> PaleGreen = ColorRgba.PaleGreen.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> MistyRose = ColorRgba.MistyRose.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> AntiqueWhite = ColorRgba.AntiqueWhite.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Linen = ColorRgba.Linen.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Beige = ColorRgba.Beige.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> WhiteSmoke = ColorRgba.WhiteSmoke.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LavenderBlush = ColorRgba.LavenderBlush.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> OldLace = ColorRgba.OldLace.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> AliceBlue = ColorRgba.AliceBlue.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Seashell = ColorRgba.Seashell.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> GhostWhite = ColorRgba.GhostWhite.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Honeydew = ColorRgba.Honeydew.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> FloralWhite = ColorRgba.FloralWhite.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Azure = ColorRgba.Azure.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> MintCream = ColorRgba.MintCream.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Snow = ColorRgba.Snow.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Ivory = ColorRgba.Ivory.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> White = ColorRgba.White.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Black = ColorRgba.Black.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkSlateGray = ColorRgba.DarkSlateGray.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DimGray = ColorRgba.DimGray.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> SlateGray = ColorRgba.SlateGray.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Gray = ColorRgba.Gray.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LightSlateGray = ColorRgba.LightSlateGray.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> DarkGray = ColorRgba.DarkGray.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Silver = ColorRgba.Silver.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> LightGray = ColorRgba.LightGray.GetRgb().ToSRgb();
	public static readonly ColorSRgb<float> Gainsboro = ColorRgba.Gainsboro.GetRgb().ToSRgb();

	public static ColorSRgb<byte> ToByte<T> ( this ColorSRgb<T> color ) where T : IFloatingPoint<T> {
		var _255 = T.CreateChecked( 255 );
		return new() {
			R = byte.CreateTruncating(color.R * _255),
			G = byte.CreateTruncating(color.G * _255),
			B = byte.CreateTruncating(color.B * _255)
		};
	}
	public static ColorSRgb<T> ToFloat<T> ( this ColorSRgb<byte> color ) where T : IFloatingPoint<T> {
		var _255 = T.CreateChecked( 255 );
		return new() {
			R = T.CreateTruncating( color.R ) / _255,
			G = T.CreateTruncating( color.G ) / _255,
			B = T.CreateTruncating( color.B ) / _255,
		};
	}

	public static ColorSRgba<T> WithOpacity<T> ( this ColorSRgb<T> color, T alpha ) where T : IFloatingPointIeee754<T>
		=> color.ToLinear().WithOpacity( alpha ).ToSRgb();

	const decimal Gamma = 2.2m;
	public static ColorRgb<T> ToLinear<T> ( this ColorSRgb<T> color ) where T : IFloatingPointIeee754<T> {
		T gamma = T.CreateSaturating( 1 / Gamma );
		return new() {
			R = T.Pow( color.R, gamma ),
			G = T.Pow( color.G, gamma ),
			B = T.Pow( color.B, gamma )
		};
	}
	public static ColorSRgb<T> ToSRgb<T> ( this ColorRgb<T> color ) where T : IFloatingPointIeee754<T> {
		T gamma = T.CreateSaturating( Gamma );
		return new() {
			R = T.Pow( color.R, gamma ),
			G = T.Pow( color.G, gamma ),
			B = T.Pow( color.B, gamma )
		};
	}
	public static ColorRgb<byte> ToLinear ( this ColorSRgb<byte> color ) {
		return new() {
			R = ColorSRgba.toLinearGammaLookup[color.R],
			G = ColorSRgba.toLinearGammaLookup[color.G],
			B = ColorSRgba.toLinearGammaLookup[color.B]
		};
	}
	public static ColorSRgb<byte> ToSRgb ( this ColorRgb<byte> color ) {
		return new() {
			R = ColorSRgba.toSRgbGammaLookup[color.R],
			G = ColorSRgba.toSRgbGammaLookup[color.G],
			B = ColorSRgba.toSRgbGammaLookup[color.B]
		};
	}

	public static ColorSRgb<T> Interpolate<T, TTime> ( this ColorSRgb<T> from, ColorSRgb<T> to, TTime time ) where T : INumber<T>, IFloatingPointIeee754<T> where TTime : INumber<TTime>, IMultiplyOperators<TTime, T, T> {
		return from.ToLinear().Interpolate( to.ToLinear(), time ).ToSRgb();
	}
}