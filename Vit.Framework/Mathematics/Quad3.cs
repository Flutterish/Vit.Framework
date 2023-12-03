/// This file [Quad3.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.FaceTemplate and parameter (4, 3) (System.ValueTuple`2[System.Int32,System.Int32])
using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Quad3<T> where T : INumber<T> {
	public Point3<T> PointA;
	public Point3<T> PointB;
	public Point3<T> PointC;
	public Point3<T> PointD;
	
	public static Quad3<T> operator * ( Quad3<T> triangle, Matrix3<T> matrix ) {
		return new() {
			PointA = triangle.PointA * matrix,
			PointB = triangle.PointB * matrix,
			PointC = triangle.PointC * matrix,
			PointD = triangle.PointD * matrix
		};
	}
	
	public readonly AxisAlignedBox3<T> BoundingBox => new() {
		MinX = T.Min( T.Min( T.Min( PointC.X, PointD.X ), PointA.X ), PointB.X ),
		MaxX = T.Max( T.Max( T.Max( PointC.X, PointD.X ), PointA.X ), PointB.X ),
		MinY = T.Min( T.Min( T.Min( PointC.Y, PointD.Y ), PointA.Y ), PointB.Y ),
		MaxY = T.Max( T.Max( T.Max( PointC.Y, PointD.Y ), PointA.Y ), PointB.Y ),
		MinZ = T.Min( T.Min( T.Min( PointC.Z, PointD.Z ), PointA.Z ), PointB.Z ),
		MaxZ = T.Max( T.Max( T.Max( PointC.Z, PointD.Z ), PointA.Z ), PointB.Z )
	};
}
