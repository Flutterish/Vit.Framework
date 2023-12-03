/// This file [Line4.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.FaceTemplate and parameter (2, 4) (System.ValueTuple`2[System.Int32,System.Int32])
using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Line4<T> where T : INumber<T> {
	public Point4<T> Start;
	public Point4<T> End;
	
	public static Line4<T> operator * ( Line4<T> quad, Matrix4<T> matrix ) {
		return new() {
			Start = quad.Start * matrix,
			End = quad.End * matrix
		};
	}
	
	public readonly AxisAlignedBox4<T> BoundingBox => new() {
		MinX = T.Min( Start.X, End.X ),
		MaxX = T.Max( Start.X, End.X ),
		MinY = T.Min( Start.Y, End.Y ),
		MaxY = T.Max( Start.Y, End.Y ),
		MinZ = T.Min( Start.Z, End.Z ),
		MaxZ = T.Max( Start.Z, End.Z ),
		MinW = T.Min( Start.W, End.W ),
		MaxW = T.Max( Start.W, End.W )
	};
}
