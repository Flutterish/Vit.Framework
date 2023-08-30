using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics;

/// <summary>
/// A color represented in the linear RGB model.
/// </summary>
/// <remarks>
/// The RBG values are linear in absolute brightness. <br/>
/// See: <see href="https://blog.johnnovak.net/2016/09/21/what-every-coder-should-know-about-gamma/#gradients"/>
/// </remarks>
public struct ColorRgb<T> : IUnlabeledColor<T>, IEqualityOperators<ColorRgb<T>, ColorRgb<T>, bool> where T : INumber<T> {
	public ReadOnlySpan<T> AsSpan () => MemoryMarshal.CreateReadOnlySpan( ref R, 3 );
	public T R;
	public T G;
	public T B;

	public ColorRgb ( T r, T g, T b ) {
		R = r;
		G = g;
		B = b;
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
		if ( typeof( T ) == typeof( byte ) ) {
			return $"#{R:X2}{G:X2}{B:X2}";
		}
		else if ( typeof( T ).IsAssignableTo( typeof( IFloatingPoint<> ) ) ) {
			var _255 = T.CreateChecked( 255 );
			var (r, g, b) = (byte.CreateTruncating( R * _255 ), byte.CreateTruncating( G * _255 ), byte.CreateTruncating( B * _255 ));
			return $"#{r:X2}{g:X2}{b:X2}";
		}
		else {
			return $"({R}, {G}, {B})";
		}
	}
}

/// <inheritdoc cref="ColorRgb{T}"/>
public static class ColorRgb {
	public static readonly ColorRgb<float> MediumVioletRed = ColorRgba.MediumVioletRed.GetRgb();
	public static readonly ColorRgb<float> DeepPink = ColorRgba.DeepPink.GetRgb();
	public static readonly ColorRgb<float> PaleVioletRed = ColorRgba.PaleVioletRed.GetRgb();
	public static readonly ColorRgb<float> HotPink = ColorRgba.HotPink.GetRgb();
	public static readonly ColorRgb<float> LightPink = ColorRgba.LightPink.GetRgb();
	public static readonly ColorRgb<float> Pink = ColorRgba.Pink.GetRgb();
	public static readonly ColorRgb<float> DarkRed = ColorRgba.DarkRed.GetRgb();
	public static readonly ColorRgb<float> Red = ColorRgba.Red.GetRgb();
	public static readonly ColorRgb<float> Firebrick = ColorRgba.Firebrick.GetRgb();
	public static readonly ColorRgb<float> Crimson = ColorRgba.Crimson.GetRgb();
	public static readonly ColorRgb<float> IndianRed = ColorRgba.IndianRed.GetRgb();
	public static readonly ColorRgb<float> LightCoral = ColorRgba.LightCoral.GetRgb();
	public static readonly ColorRgb<float> Salmon = ColorRgba.Salmon.GetRgb();
	public static readonly ColorRgb<float> DarkSalmon = ColorRgba.DarkSalmon.GetRgb();
	public static readonly ColorRgb<float> LightSalmon = ColorRgba.LightSalmon.GetRgb();
	public static readonly ColorRgb<float> OrangeRed = ColorRgba.OrangeRed.GetRgb();
	public static readonly ColorRgb<float> Tomato = ColorRgba.Tomato.GetRgb();
	public static readonly ColorRgb<float> DarkOrange = ColorRgba.DarkOrange.GetRgb();
	public static readonly ColorRgb<float> Coral = ColorRgba.Coral.GetRgb();
	public static readonly ColorRgb<float> Orange = ColorRgba.Orange.GetRgb();
	public static readonly ColorRgb<float> DarkKhaki = ColorRgba.DarkKhaki.GetRgb();
	public static readonly ColorRgb<float> Gold = ColorRgba.Gold.GetRgb();
	public static readonly ColorRgb<float> Khaki = ColorRgba.Khaki.GetRgb();
	public static readonly ColorRgb<float> PeachPuff = ColorRgba.PeachPuff.GetRgb();
	public static readonly ColorRgb<float> Yellow = ColorRgba.Yellow.GetRgb();
	public static readonly ColorRgb<float> PaleGoldenrod = ColorRgba.PaleGoldenrod.GetRgb();
	public static readonly ColorRgb<float> Moccasin = ColorRgba.Moccasin.GetRgb();
	public static readonly ColorRgb<float> PapayaWhip = ColorRgba.PapayaWhip.GetRgb();
	public static readonly ColorRgb<float> LightGoldenrodYellow = ColorRgba.LightGoldenrodYellow.GetRgb();
	public static readonly ColorRgb<float> LemonChiffon = ColorRgba.LemonChiffon.GetRgb();
	public static readonly ColorRgb<float> LightYellow = ColorRgba.LightYellow.GetRgb();
	public static readonly ColorRgb<float> Maroon = ColorRgba.Maroon.GetRgb();
	public static readonly ColorRgb<float> Brown = ColorRgba.Brown.GetRgb();
	public static readonly ColorRgb<float> SaddleBrown = ColorRgba.SaddleBrown.GetRgb();
	public static readonly ColorRgb<float> Sienna = ColorRgba.Sienna.GetRgb();
	public static readonly ColorRgb<float> Chocolate = ColorRgba.Chocolate.GetRgb();
	public static readonly ColorRgb<float> DarkGoldenrod = ColorRgba.DarkGoldenrod.GetRgb();
	public static readonly ColorRgb<float> Peru = ColorRgba.Peru.GetRgb();
	public static readonly ColorRgb<float> RosyBrown = ColorRgba.RosyBrown.GetRgb();
	public static readonly ColorRgb<float> Goldenrod = ColorRgba.Goldenrod.GetRgb();
	public static readonly ColorRgb<float> SandyBrown = ColorRgba.SandyBrown.GetRgb();
	public static readonly ColorRgb<float> Tan = ColorRgba.Tan.GetRgb();
	public static readonly ColorRgb<float> Burlywood = ColorRgba.Burlywood.GetRgb();
	public static readonly ColorRgb<float> Wheat = ColorRgba.Wheat.GetRgb();
	public static readonly ColorRgb<float> NavajoWhite = ColorRgba.NavajoWhite.GetRgb();
	public static readonly ColorRgb<float> Bisque = ColorRgba.Bisque.GetRgb();
	public static readonly ColorRgb<float> BlanchedAlmond = ColorRgba.BlanchedAlmond.GetRgb();
	public static readonly ColorRgb<float> Cornsilk = ColorRgba.Cornsilk.GetRgb();
	public static readonly ColorRgb<float> Indigo = ColorRgba.Indigo.GetRgb();
	public static readonly ColorRgb<float> Purple = ColorRgba.Purple.GetRgb();
	public static readonly ColorRgb<float> DarkMagenta = ColorRgba.DarkMagenta.GetRgb();
	public static readonly ColorRgb<float> DarkViolet = ColorRgba.DarkViolet.GetRgb();
	public static readonly ColorRgb<float> DarkSlateBlue = ColorRgba.DarkSlateBlue.GetRgb();
	public static readonly ColorRgb<float> BlueViolet = ColorRgba.BlueViolet.GetRgb();
	public static readonly ColorRgb<float> DarkOrchid = ColorRgba.DarkOrchid.GetRgb();
	public static readonly ColorRgb<float> Fuchsia = ColorRgba.Fuchsia.GetRgb();
	public static readonly ColorRgb<float> Magenta = ColorRgba.Magenta.GetRgb();
	public static readonly ColorRgb<float> SlateBlue = ColorRgba.SlateBlue.GetRgb();
	public static readonly ColorRgb<float> MediumSlateBlue = ColorRgba.MediumSlateBlue.GetRgb();
	public static readonly ColorRgb<float> MediumOrchid = ColorRgba.MediumOrchid.GetRgb();
	public static readonly ColorRgb<float> MediumPurple = ColorRgba.MediumPurple.GetRgb();
	public static readonly ColorRgb<float> Orchid = ColorRgba.Orchid.GetRgb();
	public static readonly ColorRgb<float> Violet = ColorRgba.Violet.GetRgb();
	public static readonly ColorRgb<float> Plum = ColorRgba.Plum.GetRgb();
	public static readonly ColorRgb<float> Thistle = ColorRgba.Thistle.GetRgb();
	public static readonly ColorRgb<float> Lavender = ColorRgba.Lavender.GetRgb();
	public static readonly ColorRgb<float> MidnightBlue = ColorRgba.MidnightBlue.GetRgb();
	public static readonly ColorRgb<float> Navy = ColorRgba.Navy.GetRgb();
	public static readonly ColorRgb<float> DarkBlue = ColorRgba.DarkBlue.GetRgb();
	public static readonly ColorRgb<float> MediumBlue = ColorRgba.MediumBlue.GetRgb();
	public static readonly ColorRgb<float> Blue = ColorRgba.Blue.GetRgb();
	public static readonly ColorRgb<float> RoyalBlue = ColorRgba.RoyalBlue.GetRgb();
	public static readonly ColorRgb<float> SteelBlue = ColorRgba.SteelBlue.GetRgb();
	public static readonly ColorRgb<float> DodgerBlue = ColorRgba.DodgerBlue.GetRgb();
	public static readonly ColorRgb<float> DeepSkyBlue = ColorRgba.DeepSkyBlue.GetRgb();
	public static readonly ColorRgb<float> CornflowerBlue = ColorRgba.CornflowerBlue.GetRgb();
	public static readonly ColorRgb<float> SkyBlue = ColorRgba.SkyBlue.GetRgb();
	public static readonly ColorRgb<float> LightSkyBlue = ColorRgba.LightSkyBlue.GetRgb();
	public static readonly ColorRgb<float> LightSteelBlue = ColorRgba.LightSteelBlue.GetRgb();
	public static readonly ColorRgb<float> LightBlue = ColorRgba.LightBlue.GetRgb();
	public static readonly ColorRgb<float> PowderBlue = ColorRgba.PowderBlue.GetRgb();
	public static readonly ColorRgb<float> Teal = ColorRgba.Teal.GetRgb();
	public static readonly ColorRgb<float> DarkCyan = ColorRgba.DarkCyan.GetRgb();
	public static readonly ColorRgb<float> LightSeaGreen = ColorRgba.LightSeaGreen.GetRgb();
	public static readonly ColorRgb<float> CadetBlue = ColorRgba.CadetBlue.GetRgb();
	public static readonly ColorRgb<float> DarkTurquoise = ColorRgba.DarkTurquoise.GetRgb();
	public static readonly ColorRgb<float> MediumTurquoise = ColorRgba.MediumTurquoise.GetRgb();
	public static readonly ColorRgb<float> Turquoise = ColorRgba.Turquoise.GetRgb();
	public static readonly ColorRgb<float> Aqua = ColorRgba.Aqua.GetRgb();
	public static readonly ColorRgb<float> Cyan = ColorRgba.Cyan.GetRgb();
	public static readonly ColorRgb<float> Aquamarine = ColorRgba.Aquamarine.GetRgb();
	public static readonly ColorRgb<float> PaleTurquoise = ColorRgba.PaleTurquoise.GetRgb();
	public static readonly ColorRgb<float> LightCyan = ColorRgba.LightCyan.GetRgb();
	public static readonly ColorRgb<float> DarkGreen = ColorRgba.DarkGreen.GetRgb();
	public static readonly ColorRgb<float> Green = ColorRgba.Green.GetRgb();
	public static readonly ColorRgb<float> DarkOliveGreen = ColorRgba.DarkOliveGreen.GetRgb();
	public static readonly ColorRgb<float> ForestGreen = ColorRgba.ForestGreen.GetRgb();
	public static readonly ColorRgb<float> SeaGreen = ColorRgba.SeaGreen.GetRgb();
	public static readonly ColorRgb<float> Olive = ColorRgba.Olive.GetRgb();
	public static readonly ColorRgb<float> OliveDrab = ColorRgba.OliveDrab.GetRgb();
	public static readonly ColorRgb<float> MediumSeaGreen = ColorRgba.MediumSeaGreen.GetRgb();
	public static readonly ColorRgb<float> LimeGreen = ColorRgba.LimeGreen.GetRgb();
	public static readonly ColorRgb<float> Lime = ColorRgba.Lime.GetRgb();
	public static readonly ColorRgb<float> SpringGreen = ColorRgba.SpringGreen.GetRgb();
	public static readonly ColorRgb<float> MediumSpringGreen = ColorRgba.MediumSpringGreen.GetRgb();
	public static readonly ColorRgb<float> DarkSeaGreen = ColorRgba.DarkSeaGreen.GetRgb();
	public static readonly ColorRgb<float> MediumAquamarine = ColorRgba.MediumAquamarine.GetRgb();
	public static readonly ColorRgb<float> YellowGreen = ColorRgba.YellowGreen.GetRgb();
	public static readonly ColorRgb<float> LawnGreen = ColorRgba.LawnGreen.GetRgb();
	public static readonly ColorRgb<float> Chartreuse = ColorRgba.Chartreuse.GetRgb();
	public static readonly ColorRgb<float> LightGreen = ColorRgba.LightGreen.GetRgb();
	public static readonly ColorRgb<float> GreenYellow = ColorRgba.GreenYellow.GetRgb();
	public static readonly ColorRgb<float> PaleGreen = ColorRgba.PaleGreen.GetRgb();
	public static readonly ColorRgb<float> MistyRose = ColorRgba.MistyRose.GetRgb();
	public static readonly ColorRgb<float> AntiqueWhite = ColorRgba.AntiqueWhite.GetRgb();
	public static readonly ColorRgb<float> Linen = ColorRgba.Linen.GetRgb();
	public static readonly ColorRgb<float> Beige = ColorRgba.Beige.GetRgb();
	public static readonly ColorRgb<float> WhiteSmoke = ColorRgba.WhiteSmoke.GetRgb();
	public static readonly ColorRgb<float> LavenderBlush = ColorRgba.LavenderBlush.GetRgb();
	public static readonly ColorRgb<float> OldLace = ColorRgba.OldLace.GetRgb();
	public static readonly ColorRgb<float> AliceBlue = ColorRgba.AliceBlue.GetRgb();
	public static readonly ColorRgb<float> Seashell = ColorRgba.Seashell.GetRgb();
	public static readonly ColorRgb<float> GhostWhite = ColorRgba.GhostWhite.GetRgb();
	public static readonly ColorRgb<float> Honeydew = ColorRgba.Honeydew.GetRgb();
	public static readonly ColorRgb<float> FloralWhite = ColorRgba.FloralWhite.GetRgb();
	public static readonly ColorRgb<float> Azure = ColorRgba.Azure.GetRgb();
	public static readonly ColorRgb<float> MintCream = ColorRgba.MintCream.GetRgb();
	public static readonly ColorRgb<float> Snow = ColorRgba.Snow.GetRgb();
	public static readonly ColorRgb<float> Ivory = ColorRgba.Ivory.GetRgb();
	public static readonly ColorRgb<float> White = ColorRgba.White.GetRgb();
	public static readonly ColorRgb<float> Black = ColorRgba.Black.GetRgb();
	public static readonly ColorRgb<float> DarkSlateGray = ColorRgba.DarkSlateGray.GetRgb();
	public static readonly ColorRgb<float> DimGray = ColorRgba.DimGray.GetRgb();
	public static readonly ColorRgb<float> SlateGray = ColorRgba.SlateGray.GetRgb();
	public static readonly ColorRgb<float> Gray = ColorRgba.Gray.GetRgb();
	public static readonly ColorRgb<float> LightSlateGray = ColorRgba.LightSlateGray.GetRgb();
	public static readonly ColorRgb<float> DarkGray = ColorRgba.DarkGray.GetRgb();
	public static readonly ColorRgb<float> Silver = ColorRgba.Silver.GetRgb();
	public static readonly ColorRgb<float> LightGray = ColorRgba.LightGray.GetRgb();
	public static readonly ColorRgb<float> Gainsboro = ColorRgba.Gainsboro.GetRgb();

	public static ColorRgb<byte> ToByte<T> ( this ColorRgb<T> color ) where T : IFloatingPoint<T> {
		var _255 = T.CreateChecked( 255 );
		return new() {
			R = byte.CreateTruncating( color.R * _255 ),
			G = byte.CreateTruncating( color.G * _255 ),
			B = byte.CreateTruncating( color.B * _255 )
		};
	}
	public static ColorRgb<T> ToFloat<T> ( this ColorRgb<byte> color ) where T : IFloatingPoint<T> {
		var _255 = T.CreateChecked( 255 );
		return new() {
			R = T.CreateTruncating( color.R ) / _255,
			G = T.CreateTruncating( color.G ) / _255,
			B = T.CreateTruncating( color.B ) / _255,
		};
	}

	public static ColorRgba<T> WithOpacity<T> ( this ColorRgb<T> color, T alpha ) where T : IFloatingPoint<T> {
		return new() {
			R = color.R * alpha,
			G = color.G * alpha,
			B = color.B * alpha,
			A = alpha
		};
	}
	public static ColorRgba<byte> WithOpacity ( this ColorRgb<byte> color, byte alpha ) {
		return new() {
			R = (byte)((float)color.R * alpha / 255),
			G = (byte)((float)color.G * alpha / 255),
			B = (byte)((float)color.B * alpha / 255),
			A = alpha
		};
	}

	public static ColorRgb<T> Interpolate<T, TTime> ( this ColorRgb<T> from, ColorRgb<T> to, TTime time ) where T : INumber<T>, IFloatingPointIeee754<T> where TTime : INumber<TTime>, IMultiplyOperators<TTime, T, T> {
		return new() {
			R = from.R.Lerp( to.R, time ),
			G = from.G.Lerp( to.G, time ),
			B = from.B.Lerp( to.B, time )
		};
	}
}