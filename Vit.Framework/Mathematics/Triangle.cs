using System.Numerics;

namespace Vit.Framework.Mathematics;

public static class Triangle {
	public static (T a, T b, T c) GetBarycentric<T> ( Point2<T> a, Point2<T> b, Point2<T> c, Point2<T> point ) where T : INumber<T> {
		var det = ( b.Y - c.Y ) * ( a.X - c.X ) + ( c.X - b.X ) * ( a.Y - c.Y );
		var A = ( ( b.Y - c.Y ) * ( point.X - c.X ) + ( c.X - b.X ) * ( point.Y - c.Y ) ) / det;
		var B = ( ( c.Y - a.Y ) * ( point.X - c.X ) + ( a.X - c.X ) * ( point.Y - c.Y ) ) / det;

		return (A, B, T.MultiplicativeIdentity - A - B);
	}

	public struct BarycentricBatch<T> where T : INumber<T> {
		T a1;
		T a2;
		T b1;
		T b2;
		Point2<T> c;
		public BarycentricBatch ( Point2<T> a, Point2<T> b, Point2<T> c ) {
			var determinantReceprical = T.MultiplicativeIdentity / (( b.Y - c.Y ) * ( a.X - c.X ) + ( c.X - b.X ) * ( a.Y - c.Y ));
			a1 = (b.Y - c.Y) * determinantReceprical;
			a2 = (c.X - b.X) * determinantReceprical;
			b1 = (c.Y - a.Y) * determinantReceprical;
			b2 = (a.X - c.X) * determinantReceprical;
			this.c = new Point2<T>(a2 * c.Y + a1 * c.X, b2 * c.Y + b1 * c.X);
		}

		public (T a, T b, T c) Calculate ( Point2<T> point ) {
			var A = a1 * point.X + a2 * point.Y - c.X;
			var B = b1 * point.X + b2 * point.Y - c.Y;

			return (A, B, T.MultiplicativeIdentity - A - B);
		}
	}
}
