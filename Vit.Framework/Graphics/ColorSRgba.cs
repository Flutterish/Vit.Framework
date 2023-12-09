using System.Numerics;
using System.Runtime.InteropServices;

namespace Vit.Framework.Graphics;

/// <summary>
/// A color represented in the standard RGB model (sRGB, gamma = 2.2) with a <b>premultiplied alpha</b> component.
/// </summary>
/// <remarks>
/// Note that this type should not be used with floationg-point numbers for storage, as they negate the benefits of this format.<br/>
/// The RBG values are linear in perceived brightness. <br/>
/// See: <see href="https://blog.johnnovak.net/2016/09/21/what-every-coder-should-know-about-gamma/#gradients"/>
/// </remarks>
public struct ColorSRgba<T> : IUnlabelledColor<T>, IEqualityOperators<ColorSRgba<T>, ColorSRgba<T>, bool> where T : INumber<T> {
	public ReadOnlySpan<T> AsSpan () => MemoryMarshal.CreateReadOnlySpan( ref R, 4 );
	public T R;
	public T G;
	public T B;
	public T A;

	public ColorSRgba ( T r, T g, T b, T a ) {
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public static bool operator == ( ColorSRgba<T> left, ColorSRgba<T> right ) {
		return left.R == right.R
			&& left.G == right.G
			&& left.B == right.B
			&& left.A == right.A;
	}

	public static bool operator != ( ColorSRgba<T> left, ColorSRgba<T> right ) {
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

/// <inheritdoc cref="ColorSRgba{T}"/>
public static class ColorSRgba {
	public static readonly ColorSRgba<float> MediumVioletRed = ColorRgba.MediumVioletRed.ToSRgb();
	public static readonly ColorSRgba<float> DeepPink = ColorRgba.DeepPink.ToSRgb();
	public static readonly ColorSRgba<float> PaleVioletRed = ColorRgba.PaleVioletRed.ToSRgb();
	public static readonly ColorSRgba<float> HotPink = ColorRgba.HotPink.ToSRgb();
	public static readonly ColorSRgba<float> LightPink = ColorRgba.LightPink.ToSRgb();
	public static readonly ColorSRgba<float> Pink = ColorRgba.Pink.ToSRgb();
	public static readonly ColorSRgba<float> DarkRed = ColorRgba.DarkRed.ToSRgb();
	public static readonly ColorSRgba<float> Red = ColorRgba.Red.ToSRgb();
	public static readonly ColorSRgba<float> Firebrick = ColorRgba.Firebrick.ToSRgb();
	public static readonly ColorSRgba<float> Crimson = ColorRgba.Crimson.ToSRgb();
	public static readonly ColorSRgba<float> IndianRed = ColorRgba.IndianRed.ToSRgb();
	public static readonly ColorSRgba<float> LightCoral = ColorRgba.LightCoral.ToSRgb();
	public static readonly ColorSRgba<float> Salmon = ColorRgba.Salmon.ToSRgb();
	public static readonly ColorSRgba<float> DarkSalmon = ColorRgba.DarkSalmon.ToSRgb();
	public static readonly ColorSRgba<float> LightSalmon = ColorRgba.LightSalmon.ToSRgb();
	public static readonly ColorSRgba<float> OrangeRed = ColorRgba.OrangeRed.ToSRgb();
	public static readonly ColorSRgba<float> Tomato = ColorRgba.Tomato.ToSRgb();
	public static readonly ColorSRgba<float> DarkOrange = ColorRgba.DarkOrange.ToSRgb();
	public static readonly ColorSRgba<float> Coral = ColorRgba.Coral.ToSRgb();
	public static readonly ColorSRgba<float> Orange = ColorRgba.Orange.ToSRgb();
	public static readonly ColorSRgba<float> DarkKhaki = ColorRgba.DarkKhaki.ToSRgb();
	public static readonly ColorSRgba<float> Gold = ColorRgba.Gold.ToSRgb();
	public static readonly ColorSRgba<float> Khaki = ColorRgba.Khaki.ToSRgb();
	public static readonly ColorSRgba<float> PeachPuff = ColorRgba.PeachPuff.ToSRgb();
	public static readonly ColorSRgba<float> Yellow = ColorRgba.Yellow.ToSRgb();
	public static readonly ColorSRgba<float> PaleGoldenrod = ColorRgba.PaleGoldenrod.ToSRgb();
	public static readonly ColorSRgba<float> Moccasin = ColorRgba.Moccasin.ToSRgb();
	public static readonly ColorSRgba<float> PapayaWhip = ColorRgba.PapayaWhip.ToSRgb();
	public static readonly ColorSRgba<float> LightGoldenrodYellow = ColorRgba.LightGoldenrodYellow.ToSRgb();
	public static readonly ColorSRgba<float> LemonChiffon = ColorRgba.LemonChiffon.ToSRgb();
	public static readonly ColorSRgba<float> LightYellow = ColorRgba.LightYellow.ToSRgb();
	public static readonly ColorSRgba<float> Maroon = ColorRgba.Maroon.ToSRgb();
	public static readonly ColorSRgba<float> Brown = ColorRgba.Brown.ToSRgb();
	public static readonly ColorSRgba<float> SaddleBrown = ColorRgba.SaddleBrown.ToSRgb();
	public static readonly ColorSRgba<float> Sienna = ColorRgba.Sienna.ToSRgb();
	public static readonly ColorSRgba<float> Chocolate = ColorRgba.Chocolate.ToSRgb();
	public static readonly ColorSRgba<float> DarkGoldenrod = ColorRgba.DarkGoldenrod.ToSRgb();
	public static readonly ColorSRgba<float> Peru = ColorRgba.Peru.ToSRgb();
	public static readonly ColorSRgba<float> RosyBrown = ColorRgba.RosyBrown.ToSRgb();
	public static readonly ColorSRgba<float> Goldenrod = ColorRgba.Goldenrod.ToSRgb();
	public static readonly ColorSRgba<float> SandyBrown = ColorRgba.SandyBrown.ToSRgb();
	public static readonly ColorSRgba<float> Tan = ColorRgba.Tan.ToSRgb();
	public static readonly ColorSRgba<float> Burlywood = ColorRgba.Burlywood.ToSRgb();
	public static readonly ColorSRgba<float> Wheat = ColorRgba.Wheat.ToSRgb();
	public static readonly ColorSRgba<float> NavajoWhite = ColorRgba.NavajoWhite.ToSRgb();
	public static readonly ColorSRgba<float> Bisque = ColorRgba.Bisque.ToSRgb();
	public static readonly ColorSRgba<float> BlanchedAlmond = ColorRgba.BlanchedAlmond.ToSRgb();
	public static readonly ColorSRgba<float> Cornsilk = ColorRgba.Cornsilk.ToSRgb();
	public static readonly ColorSRgba<float> Indigo = ColorRgba.Indigo.ToSRgb();
	public static readonly ColorSRgba<float> Purple = ColorRgba.Purple.ToSRgb();
	public static readonly ColorSRgba<float> DarkMagenta = ColorRgba.DarkMagenta.ToSRgb();
	public static readonly ColorSRgba<float> DarkViolet = ColorRgba.DarkViolet.ToSRgb();
	public static readonly ColorSRgba<float> DarkSlateBlue = ColorRgba.DarkSlateBlue.ToSRgb();
	public static readonly ColorSRgba<float> BlueViolet = ColorRgba.BlueViolet.ToSRgb();
	public static readonly ColorSRgba<float> DarkOrchid = ColorRgba.DarkOrchid.ToSRgb();
	public static readonly ColorSRgba<float> Fuchsia = ColorRgba.Fuchsia.ToSRgb();
	public static readonly ColorSRgba<float> Magenta = ColorRgba.Magenta.ToSRgb();
	public static readonly ColorSRgba<float> SlateBlue = ColorRgba.SlateBlue.ToSRgb();
	public static readonly ColorSRgba<float> MediumSlateBlue = ColorRgba.MediumSlateBlue.ToSRgb();
	public static readonly ColorSRgba<float> MediumOrchid = ColorRgba.MediumOrchid.ToSRgb();
	public static readonly ColorSRgba<float> MediumPurple = ColorRgba.MediumPurple.ToSRgb();
	public static readonly ColorSRgba<float> Orchid = ColorRgba.Orchid.ToSRgb();
	public static readonly ColorSRgba<float> Violet = ColorRgba.Violet.ToSRgb();
	public static readonly ColorSRgba<float> Plum = ColorRgba.Plum.ToSRgb();
	public static readonly ColorSRgba<float> Thistle = ColorRgba.Thistle.ToSRgb();
	public static readonly ColorSRgba<float> Lavender = ColorRgba.Lavender.ToSRgb();
	public static readonly ColorSRgba<float> MidnightBlue = ColorRgba.MidnightBlue.ToSRgb();
	public static readonly ColorSRgba<float> Navy = ColorRgba.Navy.ToSRgb();
	public static readonly ColorSRgba<float> DarkBlue = ColorRgba.DarkBlue.ToSRgb();
	public static readonly ColorSRgba<float> MediumBlue = ColorRgba.MediumBlue.ToSRgb();
	public static readonly ColorSRgba<float> Blue = ColorRgba.Blue.ToSRgb();
	public static readonly ColorSRgba<float> RoyalBlue = ColorRgba.RoyalBlue.ToSRgb();
	public static readonly ColorSRgba<float> SteelBlue = ColorRgba.SteelBlue.ToSRgb();
	public static readonly ColorSRgba<float> DodgerBlue = ColorRgba.DodgerBlue.ToSRgb();
	public static readonly ColorSRgba<float> DeepSkyBlue = ColorRgba.DeepSkyBlue.ToSRgb();
	public static readonly ColorSRgba<float> CornflowerBlue = ColorRgba.CornflowerBlue.ToSRgb();
	public static readonly ColorSRgba<float> SkyBlue = ColorRgba.SkyBlue.ToSRgb();
	public static readonly ColorSRgba<float> LightSkyBlue = ColorRgba.LightSkyBlue.ToSRgb();
	public static readonly ColorSRgba<float> LightSteelBlue = ColorRgba.LightSteelBlue.ToSRgb();
	public static readonly ColorSRgba<float> LightBlue = ColorRgba.LightBlue.ToSRgb();
	public static readonly ColorSRgba<float> PowderBlue = ColorRgba.PowderBlue.ToSRgb();
	public static readonly ColorSRgba<float> Teal = ColorRgba.Teal.ToSRgb();
	public static readonly ColorSRgba<float> DarkCyan = ColorRgba.DarkCyan.ToSRgb();
	public static readonly ColorSRgba<float> LightSeaGreen = ColorRgba.LightSeaGreen.ToSRgb();
	public static readonly ColorSRgba<float> CadetBlue = ColorRgba.CadetBlue.ToSRgb();
	public static readonly ColorSRgba<float> DarkTurquoise = ColorRgba.DarkTurquoise.ToSRgb();
	public static readonly ColorSRgba<float> MediumTurquoise = ColorRgba.MediumTurquoise.ToSRgb();
	public static readonly ColorSRgba<float> Turquoise = ColorRgba.Turquoise.ToSRgb();
	public static readonly ColorSRgba<float> Aqua = ColorRgba.Aqua.ToSRgb();
	public static readonly ColorSRgba<float> Cyan = ColorRgba.Cyan.ToSRgb();
	public static readonly ColorSRgba<float> Aquamarine = ColorRgba.Aquamarine.ToSRgb();
	public static readonly ColorSRgba<float> PaleTurquoise = ColorRgba.PaleTurquoise.ToSRgb();
	public static readonly ColorSRgba<float> LightCyan = ColorRgba.LightCyan.ToSRgb();
	public static readonly ColorSRgba<float> DarkGreen = ColorRgba.DarkGreen.ToSRgb();
	public static readonly ColorSRgba<float> Green = ColorRgba.Green.ToSRgb();
	public static readonly ColorSRgba<float> DarkOliveGreen = ColorRgba.DarkOliveGreen.ToSRgb();
	public static readonly ColorSRgba<float> ForestGreen = ColorRgba.ForestGreen.ToSRgb();
	public static readonly ColorSRgba<float> SeaGreen = ColorRgba.SeaGreen.ToSRgb();
	public static readonly ColorSRgba<float> Olive = ColorRgba.Olive.ToSRgb();
	public static readonly ColorSRgba<float> OliveDrab = ColorRgba.OliveDrab.ToSRgb();
	public static readonly ColorSRgba<float> MediumSeaGreen = ColorRgba.MediumSeaGreen.ToSRgb();
	public static readonly ColorSRgba<float> LimeGreen = ColorRgba.LimeGreen.ToSRgb();
	public static readonly ColorSRgba<float> Lime = ColorRgba.Lime.ToSRgb();
	public static readonly ColorSRgba<float> SpringGreen = ColorRgba.SpringGreen.ToSRgb();
	public static readonly ColorSRgba<float> MediumSpringGreen = ColorRgba.MediumSpringGreen.ToSRgb();
	public static readonly ColorSRgba<float> DarkSeaGreen = ColorRgba.DarkSeaGreen.ToSRgb();
	public static readonly ColorSRgba<float> MediumAquamarine = ColorRgba.MediumAquamarine.ToSRgb();
	public static readonly ColorSRgba<float> YellowGreen = ColorRgba.YellowGreen.ToSRgb();
	public static readonly ColorSRgba<float> LawnGreen = ColorRgba.LawnGreen.ToSRgb();
	public static readonly ColorSRgba<float> Chartreuse = ColorRgba.Chartreuse.ToSRgb();
	public static readonly ColorSRgba<float> LightGreen = ColorRgba.LightGreen.ToSRgb();
	public static readonly ColorSRgba<float> GreenYellow = ColorRgba.GreenYellow.ToSRgb();
	public static readonly ColorSRgba<float> PaleGreen = ColorRgba.PaleGreen.ToSRgb();
	public static readonly ColorSRgba<float> MistyRose = ColorRgba.MistyRose.ToSRgb();
	public static readonly ColorSRgba<float> AntiqueWhite = ColorRgba.AntiqueWhite.ToSRgb();
	public static readonly ColorSRgba<float> Linen = ColorRgba.Linen.ToSRgb();
	public static readonly ColorSRgba<float> Beige = ColorRgba.Beige.ToSRgb();
	public static readonly ColorSRgba<float> WhiteSmoke = ColorRgba.WhiteSmoke.ToSRgb();
	public static readonly ColorSRgba<float> LavenderBlush = ColorRgba.LavenderBlush.ToSRgb();
	public static readonly ColorSRgba<float> OldLace = ColorRgba.OldLace.ToSRgb();
	public static readonly ColorSRgba<float> AliceBlue = ColorRgba.AliceBlue.ToSRgb();
	public static readonly ColorSRgba<float> Seashell = ColorRgba.Seashell.ToSRgb();
	public static readonly ColorSRgba<float> GhostWhite = ColorRgba.GhostWhite.ToSRgb();
	public static readonly ColorSRgba<float> Honeydew = ColorRgba.Honeydew.ToSRgb();
	public static readonly ColorSRgba<float> FloralWhite = ColorRgba.FloralWhite.ToSRgb();
	public static readonly ColorSRgba<float> Azure = ColorRgba.Azure.ToSRgb();
	public static readonly ColorSRgba<float> MintCream = ColorRgba.MintCream.ToSRgb();
	public static readonly ColorSRgba<float> Snow = ColorRgba.Snow.ToSRgb();
	public static readonly ColorSRgba<float> Ivory = ColorRgba.Ivory.ToSRgb();
	public static readonly ColorSRgba<float> White = ColorRgba.White.ToSRgb();
	public static readonly ColorSRgba<float> Black = ColorRgba.Black.ToSRgb();
	public static readonly ColorSRgba<float> DarkSlateGray = ColorRgba.DarkSlateGray.ToSRgb();
	public static readonly ColorSRgba<float> DimGray = ColorRgba.DimGray.ToSRgb();
	public static readonly ColorSRgba<float> SlateGray = ColorRgba.SlateGray.ToSRgb();
	public static readonly ColorSRgba<float> Gray = ColorRgba.Gray.ToSRgb();
	public static readonly ColorSRgba<float> LightSlateGray = ColorRgba.LightSlateGray.ToSRgb();
	public static readonly ColorSRgba<float> DarkGray = ColorRgba.DarkGray.ToSRgb();
	public static readonly ColorSRgba<float> Silver = ColorRgba.Silver.ToSRgb();
	public static readonly ColorSRgba<float> LightGray = ColorRgba.LightGray.ToSRgb();
	public static readonly ColorSRgba<float> Gainsboro = ColorRgba.Gainsboro.ToSRgb();

	public static readonly ColorSRgba<float> Transparent = ColorRgba.Transparent.ToSRgb();

	public static ColorSRgba<byte> ToByte<T> ( this ColorSRgba<T> color ) where T : IFloatingPoint<T> {
		var _255 = T.CreateChecked( 255 );
		return new() {
			R = byte.CreateTruncating( color.R * _255 ),
			G = byte.CreateTruncating( color.G * _255 ),
			B = byte.CreateTruncating( color.B * _255 ),
			A = byte.CreateTruncating( color.A * _255 )
		};
	}
	public static ColorSRgba<T> ToFloat<T> ( this ColorSRgba<byte> color ) where T : IFloatingPoint<T> {
		var _255 = T.CreateChecked( 255 );
		return new() {
			R = T.CreateTruncating( color.R ) / _255,
			G = T.CreateTruncating( color.G ) / _255,
			B = T.CreateTruncating( color.B ) / _255,
			A = T.CreateTruncating( color.A ) / _255
		};
	}

	public static ColorSRgb<T> GetRgb<T> ( this ColorSRgba<T> color ) where T : IFloatingPointIeee754<T>
		=> color.ToLinear().GetRgb().ToSRgb();
	public static ColorSRgba<T> WithRgb<T> ( this ColorSRgba<T> color, ColorSRgb<T> value ) where T : IFloatingPointIeee754<T>
		=> color.ToLinear().WithRgb( value.ToLinear() ).ToSRgb();

	public static ColorSRgba<T> WithOpacity<T> ( this ColorSRgba<T> color, T alpha ) where T : IFloatingPointIeee754<T>
		=> color.ToLinear().WithOpacity( alpha ).ToSRgb();

	const decimal Gamma = 2.2m;
	internal static readonly byte[] toLinearGammaLookup;
	internal static readonly byte[] toSRgbGammaLookup;
	static ColorSRgba () {
		toLinearGammaLookup = new byte[256];
		toSRgbGammaLookup = new byte[256];
		for ( int i = 0; i < 256; i++ ) {
			toLinearGammaLookup[i] = (byte)(double.Pow( ((double)i) / 255, 1 / (double)Gamma ) * 255);
			toSRgbGammaLookup[i] = (byte)(double.Pow( ((double)i) / 255, (double)Gamma ) * 255);
		}
	}
	public static ColorRgba<T> ToLinear<T> ( this ColorSRgba<T> color ) where T : IFloatingPointIeee754<T> {
		T gamma = T.CreateSaturating( 1 / Gamma );
		return new() {
			R = T.Pow( color.R, gamma ),
			G = T.Pow( color.G, gamma ),
			B = T.Pow( color.B, gamma ),
			A = color.A
		};
	}
	public static ColorSRgba<T> ToSRgb<T> ( this ColorRgba<T> color ) where T : IFloatingPointIeee754<T> {
		T gamma = T.CreateSaturating( Gamma );
		return new() {
			R = T.Pow( color.R, gamma ),
			G = T.Pow( color.G, gamma ),
			B = T.Pow( color.B, gamma ),
			A = color.A
		};
	}

	public static ColorRgba<byte> ToLinear ( this ColorSRgba<byte> color ) {
		return new() {
			R = toLinearGammaLookup[color.R],
			G = toLinearGammaLookup[color.G],
			B = toLinearGammaLookup[color.B],
			A = color.A
		};
	}
	public static ColorSRgba<byte> ToSRgb ( this ColorRgba<byte> color ) {
		return new() {
			R = toSRgbGammaLookup[color.R],
			G = toSRgbGammaLookup[color.G],
			B = toSRgbGammaLookup[color.B],
			A = color.A
		};
	}

	public static ColorSRgba<T> Interpolate<T, TTime> ( this ColorSRgba<T> from, ColorSRgba<T> to, TTime time ) where T : INumber<T>, IFloatingPointIeee754<T> where TTime : INumber<TTime>, IMultiplyOperators<TTime, T, T> {
		return from.ToLinear().Interpolate( to.ToLinear(), time ).ToSRgb();
	}
}