using System.Numerics;

namespace Vit.Framework.Mathematics.Curves;

public enum CurveType {
	Custom,
	Line,
	BezierQuadratic,
	BezierCubic
}

public static class CurveTypeExtensions {
	public static T LineMin<T> ( T a, T b ) where T : INumber<T> {
		return T.Min( a, b );
	}
	public static T LineMax<T> ( T a, T b ) where T : INumber<T> {
		return T.Max( a, b );
	}

	public static T QuadraticBezierMin<T> ( T a, T b, T c ) where T : INumber<T> {
		var two = T.One + T.One;

		var A = a - two * b + c;
		var B = two * (b - a);
		var C = a;

		var tipX = -B / ( two * A );
		var min = T.Min( a, c );
		if ( tipX > T.Zero && tipX < T.One ) {
			var tipY = A * tipX * tipX + B * tipX + C;
			min = T.Min( min, tipY );
		}
		return min;
	}
	public static T QuadraticBezierMax<T> ( T a, T b, T c ) where T : INumber<T> {
		return -QuadraticBezierMin( -a, -b, -c );
	}

	public static T CubicBezierMin<T> ( T a, T b, T c, T d ) where T : INumber<T>, IFloatingPointIeee754<T> {
		var two = T.One + T.One;
		var three = two + T.One;
		var four = two + two;
		var six = four + two;
		var twelve = six + six;

		T A = d + three * (b - c) - a;
		T B = three * (a + c) - six * b;
		T C = three * (b - a);
		T D = a;

		T eval ( T x ) {
			return ((A * x + B) * x + C) * x + D;
		}

		var min = T.Min( a, d );
		var delta = four * B * B - twelve * A * C;
		if ( T.IsNegative( delta ) )
			return min;

		var sqrtDelta = T.Sqrt( delta );

		var x1 = ( sqrtDelta - two * B ) / ( six * A );
		var x2 = ( -sqrtDelta - two * B ) / ( six * A );

		if ( x1 > T.Zero && x1 < T.One ) {
			min = T.Min( min, eval( x1 ) );
		}
		if ( x2 > T.Zero && x2 < T.One ) {
			min = T.Min( min, eval( x2 ) );
		}
		return min;
	}
	public static T CubicBezierMax<T> ( T a, T b, T c, T d ) where T : INumber<T>, IFloatingPointIeee754<T> {
		return -CubicBezierMin( -a, -b, -c, -d );
	}
}