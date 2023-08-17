using System.Numerics;

namespace Vit.Framework.Graphics;

/// <summary>
/// A color represented in the linear RGB model.
/// </summary>
/// <remarks>
/// The RBG values are linear in absolute brightness. <br/>
/// See: <see href="https://blog.johnnovak.net/2016/09/21/what-every-coder-should-know-about-gamma/#gradients"/>
/// </remarks>
public struct LinearColorRgb<T> where T : INumber<T> {
	public T R;
	public T G;
	public T B;

	public LinearColorRgb ( T r, T g, T b ) {
		R = r;
		G = g;
		B = b;
	}

	public LinearColorRgba<T> WithOpacity ( T alpha ) => new() {
		R = R,
		G = G,
		B = B,
		A = alpha
	};

	public static LinearColorRgb<T> operator / ( LinearColorRgb<T> color, T scalar ) {
		return new() {
			R = color.R / scalar,
			G = color.G / scalar,
			B = color.B / scalar
		};
	}
}

/// <inheritdoc cref="LinearColorRgb{T}"/>
public static class LinearColorRgb {
	public static readonly LinearColorRgb<float> MediumVioletRed = LinearColorRgba.MediumVioletRed.Rgb;
	public static readonly LinearColorRgb<float> DeepPink = LinearColorRgba.DeepPink.Rgb;
	public static readonly LinearColorRgb<float> PaleVioletRed = LinearColorRgba.PaleVioletRed.Rgb;
	public static readonly LinearColorRgb<float> HotPink = LinearColorRgba.HotPink.Rgb;
	public static readonly LinearColorRgb<float> LightPink = LinearColorRgba.LightPink.Rgb;
	public static readonly LinearColorRgb<float> Pink = LinearColorRgba.Pink.Rgb;
	public static readonly LinearColorRgb<float> DarkRed = LinearColorRgba.DarkRed.Rgb;
	public static readonly LinearColorRgb<float> Red = LinearColorRgba.Red.Rgb;
	public static readonly LinearColorRgb<float> Firebrick = LinearColorRgba.Firebrick.Rgb;
	public static readonly LinearColorRgb<float> Crimson = LinearColorRgba.Crimson.Rgb;
	public static readonly LinearColorRgb<float> IndianRed = LinearColorRgba.IndianRed.Rgb;
	public static readonly LinearColorRgb<float> LightCoral = LinearColorRgba.LightCoral.Rgb;
	public static readonly LinearColorRgb<float> Salmon = LinearColorRgba.Salmon.Rgb;
	public static readonly LinearColorRgb<float> DarkSalmon = LinearColorRgba.DarkSalmon.Rgb;
	public static readonly LinearColorRgb<float> LightSalmon = LinearColorRgba.LightSalmon.Rgb;
	public static readonly LinearColorRgb<float> OrangeRed = LinearColorRgba.OrangeRed.Rgb;
	public static readonly LinearColorRgb<float> Tomato = LinearColorRgba.Tomato.Rgb;
	public static readonly LinearColorRgb<float> DarkOrange = LinearColorRgba.DarkOrange.Rgb;
	public static readonly LinearColorRgb<float> Coral = LinearColorRgba.Coral.Rgb;
	public static readonly LinearColorRgb<float> Orange = LinearColorRgba.Orange.Rgb;
	public static readonly LinearColorRgb<float> DarkKhaki = LinearColorRgba.DarkKhaki.Rgb;
	public static readonly LinearColorRgb<float> Gold = LinearColorRgba.Gold.Rgb;
	public static readonly LinearColorRgb<float> Khaki = LinearColorRgba.Khaki.Rgb;
	public static readonly LinearColorRgb<float> PeachPuff = LinearColorRgba.PeachPuff.Rgb;
	public static readonly LinearColorRgb<float> Yellow = LinearColorRgba.Yellow.Rgb;
	public static readonly LinearColorRgb<float> PaleGoldenrod = LinearColorRgba.PaleGoldenrod.Rgb;
	public static readonly LinearColorRgb<float> Moccasin = LinearColorRgba.Moccasin.Rgb;
	public static readonly LinearColorRgb<float> PapayaWhip = LinearColorRgba.PapayaWhip.Rgb;
	public static readonly LinearColorRgb<float> LightGoldenrodYellow = LinearColorRgba.LightGoldenrodYellow.Rgb;
	public static readonly LinearColorRgb<float> LemonChiffon = LinearColorRgba.LemonChiffon.Rgb;
	public static readonly LinearColorRgb<float> LightYellow = LinearColorRgba.LightYellow.Rgb;
	public static readonly LinearColorRgb<float> Maroon = LinearColorRgba.Maroon.Rgb;
	public static readonly LinearColorRgb<float> Brown = LinearColorRgba.Brown.Rgb;
	public static readonly LinearColorRgb<float> SaddleBrown = LinearColorRgba.SaddleBrown.Rgb;
	public static readonly LinearColorRgb<float> Sienna = LinearColorRgba.Sienna.Rgb;
	public static readonly LinearColorRgb<float> Chocolate = LinearColorRgba.Chocolate.Rgb;
	public static readonly LinearColorRgb<float> DarkGoldenrod = LinearColorRgba.DarkGoldenrod.Rgb;
	public static readonly LinearColorRgb<float> Peru = LinearColorRgba.Peru.Rgb;
	public static readonly LinearColorRgb<float> RosyBrown = LinearColorRgba.RosyBrown.Rgb;
	public static readonly LinearColorRgb<float> Goldenrod = LinearColorRgba.Goldenrod.Rgb;
	public static readonly LinearColorRgb<float> SandyBrown = LinearColorRgba.SandyBrown.Rgb;
	public static readonly LinearColorRgb<float> Tan = LinearColorRgba.Tan.Rgb;
	public static readonly LinearColorRgb<float> Burlywood = LinearColorRgba.Burlywood.Rgb;
	public static readonly LinearColorRgb<float> Wheat = LinearColorRgba.Wheat.Rgb;
	public static readonly LinearColorRgb<float> NavajoWhite = LinearColorRgba.NavajoWhite.Rgb;
	public static readonly LinearColorRgb<float> Bisque = LinearColorRgba.Bisque.Rgb;
	public static readonly LinearColorRgb<float> BlanchedAlmond = LinearColorRgba.BlanchedAlmond.Rgb;
	public static readonly LinearColorRgb<float> Cornsilk = LinearColorRgba.Cornsilk.Rgb;
	public static readonly LinearColorRgb<float> Indigo = LinearColorRgba.Indigo.Rgb;
	public static readonly LinearColorRgb<float> Purple = LinearColorRgba.Purple.Rgb;
	public static readonly LinearColorRgb<float> DarkMagenta = LinearColorRgba.DarkMagenta.Rgb;
	public static readonly LinearColorRgb<float> DarkViolet = LinearColorRgba.DarkViolet.Rgb;
	public static readonly LinearColorRgb<float> DarkSlateBlue = LinearColorRgba.DarkSlateBlue.Rgb;
	public static readonly LinearColorRgb<float> BlueViolet = LinearColorRgba.BlueViolet.Rgb;
	public static readonly LinearColorRgb<float> DarkOrchid = LinearColorRgba.DarkOrchid.Rgb;
	public static readonly LinearColorRgb<float> Fuchsia = LinearColorRgba.Fuchsia.Rgb;
	public static readonly LinearColorRgb<float> Magenta = LinearColorRgba.Magenta.Rgb;
	public static readonly LinearColorRgb<float> SlateBlue = LinearColorRgba.SlateBlue.Rgb;
	public static readonly LinearColorRgb<float> MediumSlateBlue = LinearColorRgba.MediumSlateBlue.Rgb;
	public static readonly LinearColorRgb<float> MediumOrchid = LinearColorRgba.MediumOrchid.Rgb;
	public static readonly LinearColorRgb<float> MediumPurple = LinearColorRgba.MediumPurple.Rgb;
	public static readonly LinearColorRgb<float> Orchid = LinearColorRgba.Orchid.Rgb;
	public static readonly LinearColorRgb<float> Violet = LinearColorRgba.Violet.Rgb;
	public static readonly LinearColorRgb<float> Plum = LinearColorRgba.Plum.Rgb;
	public static readonly LinearColorRgb<float> Thistle = LinearColorRgba.Thistle.Rgb;
	public static readonly LinearColorRgb<float> Lavender = LinearColorRgba.Lavender.Rgb;
	public static readonly LinearColorRgb<float> MidnightBlue = LinearColorRgba.MidnightBlue.Rgb;
	public static readonly LinearColorRgb<float> Navy = LinearColorRgba.Navy.Rgb;
	public static readonly LinearColorRgb<float> DarkBlue = LinearColorRgba.DarkBlue.Rgb;
	public static readonly LinearColorRgb<float> MediumBlue = LinearColorRgba.MediumBlue.Rgb;
	public static readonly LinearColorRgb<float> Blue = LinearColorRgba.Blue.Rgb;
	public static readonly LinearColorRgb<float> RoyalBlue = LinearColorRgba.RoyalBlue.Rgb;
	public static readonly LinearColorRgb<float> SteelBlue = LinearColorRgba.SteelBlue.Rgb;
	public static readonly LinearColorRgb<float> DodgerBlue = LinearColorRgba.DodgerBlue.Rgb;
	public static readonly LinearColorRgb<float> DeepSkyBlue = LinearColorRgba.DeepSkyBlue.Rgb;
	public static readonly LinearColorRgb<float> CornflowerBlue = LinearColorRgba.CornflowerBlue.Rgb;
	public static readonly LinearColorRgb<float> SkyBlue = LinearColorRgba.SkyBlue.Rgb;
	public static readonly LinearColorRgb<float> LightSkyBlue = LinearColorRgba.LightSkyBlue.Rgb;
	public static readonly LinearColorRgb<float> LightSteelBlue = LinearColorRgba.LightSteelBlue.Rgb;
	public static readonly LinearColorRgb<float> LightBlue = LinearColorRgba.LightBlue.Rgb;
	public static readonly LinearColorRgb<float> PowderBlue = LinearColorRgba.PowderBlue.Rgb;
	public static readonly LinearColorRgb<float> Teal = LinearColorRgba.Teal.Rgb;
	public static readonly LinearColorRgb<float> DarkCyan = LinearColorRgba.DarkCyan.Rgb;
	public static readonly LinearColorRgb<float> LightSeaGreen = LinearColorRgba.LightSeaGreen.Rgb;
	public static readonly LinearColorRgb<float> CadetBlue = LinearColorRgba.CadetBlue.Rgb;
	public static readonly LinearColorRgb<float> DarkTurquoise = LinearColorRgba.DarkTurquoise.Rgb;
	public static readonly LinearColorRgb<float> MediumTurquoise = LinearColorRgba.MediumTurquoise.Rgb;
	public static readonly LinearColorRgb<float> Turquoise = LinearColorRgba.Turquoise.Rgb;
	public static readonly LinearColorRgb<float> Aqua = LinearColorRgba.Aqua.Rgb;
	public static readonly LinearColorRgb<float> Cyan = LinearColorRgba.Cyan.Rgb;
	public static readonly LinearColorRgb<float> Aquamarine = LinearColorRgba.Aquamarine.Rgb;
	public static readonly LinearColorRgb<float> PaleTurquoise = LinearColorRgba.PaleTurquoise.Rgb;
	public static readonly LinearColorRgb<float> LightCyan = LinearColorRgba.LightCyan.Rgb;
	public static readonly LinearColorRgb<float> DarkGreen = LinearColorRgba.DarkGreen.Rgb;
	public static readonly LinearColorRgb<float> Green = LinearColorRgba.Green.Rgb;
	public static readonly LinearColorRgb<float> DarkOliveGreen = LinearColorRgba.DarkOliveGreen.Rgb;
	public static readonly LinearColorRgb<float> ForestGreen = LinearColorRgba.ForestGreen.Rgb;
	public static readonly LinearColorRgb<float> SeaGreen = LinearColorRgba.SeaGreen.Rgb;
	public static readonly LinearColorRgb<float> Olive = LinearColorRgba.Olive.Rgb;
	public static readonly LinearColorRgb<float> OliveDrab = LinearColorRgba.OliveDrab.Rgb;
	public static readonly LinearColorRgb<float> MediumSeaGreen = LinearColorRgba.MediumSeaGreen.Rgb;
	public static readonly LinearColorRgb<float> LimeGreen = LinearColorRgba.LimeGreen.Rgb;
	public static readonly LinearColorRgb<float> Lime = LinearColorRgba.Lime.Rgb;
	public static readonly LinearColorRgb<float> SpringGreen = LinearColorRgba.SpringGreen.Rgb;
	public static readonly LinearColorRgb<float> MediumSpringGreen = LinearColorRgba.MediumSpringGreen.Rgb;
	public static readonly LinearColorRgb<float> DarkSeaGreen = LinearColorRgba.DarkSeaGreen.Rgb;
	public static readonly LinearColorRgb<float> MediumAquamarine = LinearColorRgba.MediumAquamarine.Rgb;
	public static readonly LinearColorRgb<float> YellowGreen = LinearColorRgba.YellowGreen.Rgb;
	public static readonly LinearColorRgb<float> LawnGreen = LinearColorRgba.LawnGreen.Rgb;
	public static readonly LinearColorRgb<float> Chartreuse = LinearColorRgba.Chartreuse.Rgb;
	public static readonly LinearColorRgb<float> LightGreen = LinearColorRgba.LightGreen.Rgb;
	public static readonly LinearColorRgb<float> GreenYellow = LinearColorRgba.GreenYellow.Rgb;
	public static readonly LinearColorRgb<float> PaleGreen = LinearColorRgba.PaleGreen.Rgb;
	public static readonly LinearColorRgb<float> MistyRose = LinearColorRgba.MistyRose.Rgb;
	public static readonly LinearColorRgb<float> AntiqueWhite = LinearColorRgba.AntiqueWhite.Rgb;
	public static readonly LinearColorRgb<float> Linen = LinearColorRgba.Linen.Rgb;
	public static readonly LinearColorRgb<float> Beige = LinearColorRgba.Beige.Rgb;
	public static readonly LinearColorRgb<float> WhiteSmoke = LinearColorRgba.WhiteSmoke.Rgb;
	public static readonly LinearColorRgb<float> LavenderBlush = LinearColorRgba.LavenderBlush.Rgb;
	public static readonly LinearColorRgb<float> OldLace = LinearColorRgba.OldLace.Rgb;
	public static readonly LinearColorRgb<float> AliceBlue = LinearColorRgba.AliceBlue.Rgb;
	public static readonly LinearColorRgb<float> Seashell = LinearColorRgba.Seashell.Rgb;
	public static readonly LinearColorRgb<float> GhostWhite = LinearColorRgba.GhostWhite.Rgb;
	public static readonly LinearColorRgb<float> Honeydew = LinearColorRgba.Honeydew.Rgb;
	public static readonly LinearColorRgb<float> FloralWhite = LinearColorRgba.FloralWhite.Rgb;
	public static readonly LinearColorRgb<float> Azure = LinearColorRgba.Azure.Rgb;
	public static readonly LinearColorRgb<float> MintCream = LinearColorRgba.MintCream.Rgb;
	public static readonly LinearColorRgb<float> Snow = LinearColorRgba.Snow.Rgb;
	public static readonly LinearColorRgb<float> Ivory = LinearColorRgba.Ivory.Rgb;
	public static readonly LinearColorRgb<float> White = LinearColorRgba.White.Rgb;
	public static readonly LinearColorRgb<float> Black = LinearColorRgba.Black.Rgb;
	public static readonly LinearColorRgb<float> DarkSlateGray = LinearColorRgba.DarkSlateGray.Rgb;
	public static readonly LinearColorRgb<float> DimGray = LinearColorRgba.DimGray.Rgb;
	public static readonly LinearColorRgb<float> SlateGray = LinearColorRgba.SlateGray.Rgb;
	public static readonly LinearColorRgb<float> Gray = LinearColorRgba.Gray.Rgb;
	public static readonly LinearColorRgb<float> LightSlateGray = LinearColorRgba.LightSlateGray.Rgb;
	public static readonly LinearColorRgb<float> DarkGray = LinearColorRgba.DarkGray.Rgb;
	public static readonly LinearColorRgb<float> Silver = LinearColorRgba.Silver.Rgb;
	public static readonly LinearColorRgb<float> LightGray = LinearColorRgba.LightGray.Rgb;
	public static readonly LinearColorRgb<float> Gainsboro = LinearColorRgba.Gainsboro.Rgb;
}