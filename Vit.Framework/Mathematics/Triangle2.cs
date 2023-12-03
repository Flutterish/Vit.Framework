/// This file [Triangle2.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.FaceTemplate and parameter (3, 2) (System.ValueTuple`2[System.Int32,System.Int32])
using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Triangle2<T> where T : INumber<T> {
	public Point2<T> PointA;
	public Point2<T> PointB;
	public Point2<T> PointC;
	
	public static Triangle2<T> operator * ( Triangle2<T> line, Matrix2<T> matrix ) {
		return new() {
			PointA = line.PointA * matrix,
			PointB = line.PointB * matrix,
			PointC = line.PointC * matrix
		};
	}
	
	public static Triangle2<T> operator * ( Triangle2<T> line, Matrix3<T> matrix ) {
		return new() {
			PointA = matrix.Apply( line.PointA ),
			PointB = matrix.Apply( line.PointB ),
			PointC = matrix.Apply( line.PointC )
		};
	}
	
	public readonly AxisAlignedBox2<T> BoundingBox => new() {
		MinX = T.Min( T.Min( PointB.X, PointC.X ), PointA.X ),
		MaxX = T.Max( T.Max( PointB.X, PointC.X ), PointA.X ),
		MinY = T.Min( T.Min( PointB.Y, PointC.Y ), PointA.Y ),
		MaxY = T.Max( T.Max( PointB.Y, PointC.Y ), PointA.Y )
	};
}
