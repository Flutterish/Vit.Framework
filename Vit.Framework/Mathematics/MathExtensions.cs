using System.Numerics;

namespace Vit.Framework.Mathematics;

public static class MathExtensions {
	public static T Mod<T> ( this T value, T mod ) where T : INumber<T> {
		return value <= T.Zero
			? value % mod + mod
			: value % mod;
	}

	public static T Lerp<T, TTime> ( this T value, T goal, TTime time )
		where T : IAdditionOperators<T, T, T>, IMultiplyOperators<T, TTime, T>
		where TTime : INumber<TTime>
	{
		return value * ( TTime.One - time ) + goal * time;
	}
}
