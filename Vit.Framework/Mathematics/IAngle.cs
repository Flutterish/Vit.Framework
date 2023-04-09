using System.Numerics;

namespace Vit.Framework.Mathematics;

public interface IAngle<TSelf, T> : 
	IMultiplyOperators<TSelf, T, TSelf>, IDivisionOperators<TSelf, T, TSelf>, 
	IAdditionOperators<TSelf, TSelf, TSelf>, ISubtractionOperators<TSelf, TSelf, TSelf>
	where TSelf : IAngle<TSelf, T>
{
	abstract static T Cos ( TSelf value );
	abstract static T Sin ( TSelf value );
	abstract static T Tan ( TSelf value );
	abstract static T Acos ( TSelf value );
	abstract static T Asin ( TSelf value );
	abstract static T Atan ( TSelf value );

	public static abstract TSelf operator * ( T left, TSelf right );
}
