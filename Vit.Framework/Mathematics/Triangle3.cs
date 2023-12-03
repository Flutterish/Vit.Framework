/// This file [Triangle3.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.FaceTemplate and parameter (3, 3) (System.ValueTuple`2[System.Int32,System.Int32])
using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Triangle3<T> where T : INumber<T> {
	public Point3<T> PointA;
	public Point3<T> PointB;
	public Point3<T> PointC;
	
	public static Triangle3<T> operator * ( Triangle3<T> triangle, Matrix3<T> matrix ) {
		return new() {
			PointA = triangle.PointA * matrix,
			PointB = triangle.PointB * matrix,
			PointC = triangle.PointC * matrix
		};
	}
	
	public readonly AxisAlignedBox3<T> BoundingBox => new() {
		MinX = T.Min( T.Min( PointB.X, PointC.X ), PointA.X ),
		MaxX = T.Max( T.Max( PointB.X, PointC.X ), PointA.X ),
		MinY = T.Min( T.Min( PointB.Y, PointC.Y ), PointA.Y ),
		MaxY = T.Max( T.Max( PointB.Y, PointC.Y ), PointA.Y ),
		MinZ = T.Min( T.Min( PointB.Z, PointC.Z ), PointA.Z ),
		MaxZ = T.Max( T.Max( PointB.Z, PointC.Z ), PointA.Z )
	};
}
