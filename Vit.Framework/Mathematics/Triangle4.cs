/// This file [Triangle4.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.FaceTemplate and parameter (3, 4) (System.ValueTuple`2[System.Int32,System.Int32])
using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Triangle4<T> where T : INumber<T> {
	public Point4<T> PointA;
	public Point4<T> PointB;
	public Point4<T> PointC;
	
	public static Triangle4<T> operator * ( Triangle4<T> quad, Matrix4<T> matrix ) {
		return new() {
			PointA = quad.PointA * matrix,
			PointB = quad.PointB * matrix,
			PointC = quad.PointC * matrix
		};
	}
	
	public readonly AxisAlignedBox4<T> BoundingBox => new() {
		MinX = T.Min( T.Min( PointB.X, PointC.X ), PointA.X ),
		MaxX = T.Max( T.Max( PointB.X, PointC.X ), PointA.X ),
		MinY = T.Min( T.Min( PointB.Y, PointC.Y ), PointA.Y ),
		MaxY = T.Max( T.Max( PointB.Y, PointC.Y ), PointA.Y ),
		MinZ = T.Min( T.Min( PointB.Z, PointC.Z ), PointA.Z ),
		MaxZ = T.Max( T.Max( PointB.Z, PointC.Z ), PointA.Z ),
		MinW = T.Min( T.Min( PointB.W, PointC.W ), PointA.W ),
		MaxW = T.Max( T.Max( PointB.W, PointC.W ), PointA.W )
	};
}
