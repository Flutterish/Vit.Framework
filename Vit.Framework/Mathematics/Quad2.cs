/// This file [Quad2.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.FaceTemplate and parameter (4, 2) (System.ValueTuple`2[System.Int32,System.Int32])
using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Quad2<T> where T : INumber<T> {
	public Point2<T> PointA;
	public Point2<T> PointB;
	public Point2<T> PointC;
	public Point2<T> PointD;
	
	public static Quad2<T> operator * ( Quad2<T> line, Matrix2<T> matrix ) {
		return new() {
			PointA = line.PointA * matrix,
			PointB = line.PointB * matrix,
			PointC = line.PointC * matrix,
			PointD = line.PointD * matrix
		};
	}
	
	public static Quad2<T> operator * ( Quad2<T> line, Matrix3<T> matrix ) {
		return new() {
			PointA = matrix.Apply( line.PointA ),
			PointB = matrix.Apply( line.PointB ),
			PointC = matrix.Apply( line.PointC ),
			PointD = matrix.Apply( line.PointD )
		};
	}
	
	public readonly AxisAlignedBox2<T> BoundingBox => new() {
		MinX = T.Min( T.Min( T.Min( PointC.X, PointD.X ), PointA.X ), PointB.X ),
		MaxX = T.Max( T.Max( T.Max( PointC.X, PointD.X ), PointA.X ), PointB.X ),
		MinY = T.Min( T.Min( T.Min( PointC.Y, PointD.Y ), PointA.Y ), PointB.Y ),
		MaxY = T.Max( T.Max( T.Max( PointC.Y, PointD.Y ), PointA.Y ), PointB.Y )
	};
}
