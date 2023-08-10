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
}
