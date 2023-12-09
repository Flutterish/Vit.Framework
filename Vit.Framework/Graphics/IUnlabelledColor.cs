using System.Numerics;

namespace Vit.Framework.Graphics;

public interface IUnlabelledColor<T> where T : INumber<T> {
	ReadOnlySpan<T> AsSpan ();
}
