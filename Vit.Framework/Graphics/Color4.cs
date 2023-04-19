using System.Numerics;

namespace Vit.Framework.Graphics;

public struct Color4<T> where T : unmanaged, INumber<T> {
	public T R;
	public T G;
	public T B;
	public T A;

	public Color4 ( T r, T g, T b, T a ) {
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public Color4 ( T r, T g, T b ) {
		R = r;
		G = g;
		B = b;
		A = T.One;
	}
}
