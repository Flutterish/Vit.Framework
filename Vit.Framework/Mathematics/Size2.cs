using System.Numerics;

namespace Vit.Framework.Mathematics;

public struct Size2<T> where T : struct, INumber<T> {
	public T Width;
	public T Height;

	public Size2 ( T width, T height ) {
		Width = width;
		Height = height;
	}
}
