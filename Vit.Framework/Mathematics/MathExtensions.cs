using System.Numerics;

namespace Vit.Framework.Mathematics;

public static class MathExtensions {
	/// <summary>
	/// True modulus, where the sign of the result is the same as the mod.
	/// </summary>
	public static T Mod<T> ( this T value, T mod ) where T : INumber<T> {
		return value < T.Zero
			? value % mod + mod
			: value % mod;
	}

	/// <summary>
	/// Linearly interpolates 2 values.
	/// </summary>
	public static T Lerp<T, TTime> ( this T value, T goal, TTime time )
		where T : INumber<T>
		where TTime : INumber<TTime>, IMultiplyOperators<TTime, T, T>
	{
		return (TTime.One - time) * value + time * goal;
	}

	/// <summary>
	/// Quadraticaly interpolates 2 values such that <c>f(0) = A, f(1) = B and f(0.5) = AB</c>.
	/// </summary>
	public static T Querp<T, TTime> ( this T value, T goal, TTime time )
		where T : INumber<T>
		where TTime : INumber<TTime>, IMultiplyOperators<TTime, T, T> {
		var c = value;
		var two = T.One + T.One;
		var a = two * ( goal - c - two * ( value * goal - c ) );
		var b = two * ( value * goal - c ) - a / two;

		return time * time * a + time * b + c;
	}
}
