using System.Numerics;

namespace Vit.Framework.Graphics;

public interface IUnlabeledColor<T> where T : INumber<T> {
	ReadOnlySpan<T> AsSpan ();
}
