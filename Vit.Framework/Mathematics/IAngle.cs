using System.Numerics;

namespace Vit.Framework.Mathematics;

public interface IAngle<TSelf, T> : 
	IMultiplyOperators<TSelf, T, TSelf>, IDivisionOperators<TSelf, T, TSelf>, IModulusOperators<TSelf, TSelf, TSelf>,
	IAdditionOperators<TSelf, TSelf, TSelf>, ISubtractionOperators<TSelf, TSelf, TSelf>, IUnaryNegationOperators<TSelf, TSelf>,
	IComparisonOperators<TSelf, TSelf, bool>, IEqualityOperators<TSelf, TSelf, bool>,
	IComparable<TSelf>, IEquatable<TSelf>
	where TSelf : IAngle<TSelf, T>
	where T : INumber<T>
{
	abstract static T Cos ( TSelf value );
	abstract static T Sin ( TSelf value );
	abstract static T Tan ( TSelf value );
	abstract static T Acos ( TSelf value );
	abstract static T Asin ( TSelf value );
	abstract static T Atan ( TSelf value );

	static abstract TSelf operator * ( T left, TSelf right );
	static abstract T operator / ( TSelf left, TSelf right );

	static abstract TSelf Zero { get; }
	static abstract TSelf FullRotation { get; }

	public virtual TSelf DeltaTo ( TSelf target ) {
		var half = TSelf.FullRotation / (T.One + T.One);
		var self = (TSelf)this % half + half;
		var other = target % half + half;

		var difference = other - self;
		if ( difference > half )
			return difference - TSelf.FullRotation;
		else if ( difference < -half )
			return difference + TSelf.FullRotation;
		return difference;
	}

	public virtual TSelf Mod ( TSelf mod ) {
		var value = (TSelf)this;
		return value <= TSelf.Zero
				? value % mod + mod
				: value % mod;
	}

	public virtual bool IsEquivalent ( TSelf other ) {
		return this.Mod( TSelf.FullRotation ) == other.Mod( TSelf.FullRotation );
	}
}
