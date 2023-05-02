using System.Numerics;

namespace Vit.Framework.Mathematics;

public static class Triangle {
	public static (T a, T b, T c) GetBarycentric<T> ( Point2<T> a, Point2<T> b, Point2<T> c, Point2<T> point ) where T : INumber<T> {
		var det = ( b.Y - c.Y ) * ( a.X - c.X ) + ( c.X - b.X ) * ( a.Y - c.Y );
		var A = ( ( b.Y - c.Y ) * ( point.X - c.X ) + ( c.X - b.X ) * ( point.Y - c.Y ) ) / det;
		var B = ( ( c.Y - a.Y ) * ( point.X - c.X ) + ( a.X - c.X ) * ( point.Y - c.Y ) ) / det;

		return (A, B, T.MultiplicativeIdentity - A - B);
	}
}
