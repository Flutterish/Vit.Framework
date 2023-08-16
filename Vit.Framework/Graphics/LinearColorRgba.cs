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
