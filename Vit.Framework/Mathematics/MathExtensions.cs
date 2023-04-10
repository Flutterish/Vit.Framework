using System.Numerics;

namespace Vit.Framework.Mathematics;

public static class MathExtensions {
	public static T Mod<T> ( this T value, T mod ) where T : INumber<T> {
		return value <= T.Zero
			? value % mod + mod
			: value % mod;
	}
}
