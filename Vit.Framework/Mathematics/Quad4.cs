/// This file [Quad4.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.FaceTemplate and parameter (4, 4) (System.ValueTuple`2[System.Int32,System.Int32])
using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Quad4<T> where T : INumber<T> {
	public Point4<T> PointA;
	public Point4<T> PointB;
	public Point4<T> PointC;
	public Point4<T> PointD;
	
	public static Quad4<T> operator * ( Quad4<T> quad, Matrix4<T> matrix ) {
		return new() {
			PointA = quad.PointA * matrix,
			PointB = quad.PointB * matrix,
			PointC = quad.PointC * matrix,
			PointD = quad.PointD * matrix
		};
	}
	
	public readonly AxisAlignedBox4<T> BoundingBox => new() {
		MinX = T.Min( T.Min( T.Min( PointC.X, PointD.X ), PointA.X ), PointB.X ),
		MaxX = T.Max( T.Max( T.Max( PointC.X, PointD.X ), PointA.X ), PointB.X ),
		MinY = T.Min( T.Min( T.Min( PointC.Y, PointD.Y ), PointA.Y ), PointB.Y ),
		MaxY = T.Max( T.Max( T.Max( PointC.Y, PointD.Y ), PointA.Y ), PointB.Y ),
		MinZ = T.Min( T.Min( T.Min( PointC.Z, PointD.Z ), PointA.Z ), PointB.Z ),
		MaxZ = T.Max( T.Max( T.Max( PointC.Z, PointD.Z ), PointA.Z ), PointB.Z ),
		MinW = T.Min( T.Min( T.Min( PointC.W, PointD.W ), PointA.W ), PointB.W ),
		MaxW = T.Max( T.Max( T.Max( PointC.W, PointD.W ), PointA.W ), PointB.W )
	};
}
