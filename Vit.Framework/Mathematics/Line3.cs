/// This file [Line3.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.FaceTemplate and parameter (2, 3) (System.ValueTuple`2[System.Int32,System.Int32])
using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Line3<T> where T : INumber<T> {
	public Point3<T> Start;
	public Point3<T> End;
	
	public static Line3<T> operator * ( Line3<T> triangle, Matrix3<T> matrix ) {
		return new() {
			Start = triangle.Start * matrix,
			End = triangle.End * matrix
		};
	}
	
	public readonly AxisAlignedBox3<T> BoundingBox => new() {
		MinX = T.Min( Start.X, End.X ),
		MaxX = T.Max( Start.X, End.X ),
		MinY = T.Min( Start.Y, End.Y ),
		MaxY = T.Max( Start.Y, End.Y ),
		MinZ = T.Min( Start.Z, End.Z ),
		MaxZ = T.Max( Start.Z, End.Z )
	};
}
