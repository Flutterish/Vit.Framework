/// This file [Line2.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.FaceTemplate and parameter (2, 2) (System.ValueTuple`2[System.Int32,System.Int32])
using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Line2<T> where T : INumber<T> {
	public Point2<T> Start;
	public Point2<T> End;
	
	public static Line2<T> operator * ( Line2<T> line, Matrix2<T> matrix ) {
		return new() {
			Start = line.Start * matrix,
			End = line.End * matrix
		};
	}
	
	public readonly AxisAlignedBox2<T> BoundingBox => new() {
		MinX = T.Min( Start.X, End.X ),
		MaxX = T.Max( Start.X, End.X ),
		MinY = T.Min( Start.Y, End.Y ),
		MaxY = T.Max( Start.Y, End.Y )
	};
}
