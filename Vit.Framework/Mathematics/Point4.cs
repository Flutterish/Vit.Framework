/// This file [Point4.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.PointTemplate and parameter 4 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Mathematics;

public struct Point4<T> : IInterpolatable<Point4<T>, T>, IEqualityOperators<Point4<T>, Point4<T>, bool>, IEquatable<Point4<T>> where T : INumber<T> {
	public T X;
	public T Y;
	public T Z;
	public T W;
	
	public Point4 ( T x, T y, T z, T w ) {
		X = x;
		Y = y;
		Z = z;
		W = w;
	}
	
	public Point4 ( T all ) {
		X = Y = Z = W = all;
	}
	
	#nullable disable
	public Point4 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly Point4<T> UnitX = new( T.One, T.Zero, T.Zero, T.Zero );
	public static readonly Point4<T> UnitY = new( T.Zero, T.One, T.Zero, T.Zero );
	public static readonly Point4<T> UnitZ = new( T.Zero, T.Zero, T.One, T.Zero );
	public static readonly Point4<T> UnitW = new( T.Zero, T.Zero, T.Zero, T.One );
	public static readonly Point4<T> One = new( T.One );
	public static readonly Point4<T> Zero = new( T.Zero );
	
	public Point2<T> YX => new( Y, X );
	public Point3<T> ZYX => new( Z, Y, X );
	public Point3<T> WYX => new( W, Y, X );
	public Point2<T> ZX => new( Z, X );
	public Point3<T> YZX => new( Y, Z, X );
	public Point3<T> WZX => new( W, Z, X );
	public Point2<T> WX => new( W, X );
	public Point3<T> YWX => new( Y, W, X );
	public Point3<T> ZWX => new( Z, W, X );
	public Point2<T> XY => new( X, Y );
	public Point3<T> ZXY => new( Z, X, Y );
	public Point3<T> WXY => new( W, X, Y );
	public Point2<T> ZY => new( Z, Y );
	public Point3<T> XZY => new( X, Z, Y );
	public Point3<T> WZY => new( W, Z, Y );
	public Point2<T> WY => new( W, Y );
	public Point3<T> XWY => new( X, W, Y );
	public Point3<T> ZWY => new( Z, W, Y );
	public Point2<T> XZ => new( X, Z );
	public Point3<T> YXZ => new( Y, X, Z );
	public Point3<T> WXZ => new( W, X, Z );
	public Point2<T> YZ => new( Y, Z );
	public Point3<T> XYZ => new( X, Y, Z );
	public Point3<T> WYZ => new( W, Y, Z );
	public Point2<T> WZ => new( W, Z );
	public Point3<T> XWZ => new( X, W, Z );
	public Point3<T> YWZ => new( Y, W, Z );
	public Point2<T> XW => new( X, W );
	public Point3<T> YXW => new( Y, X, W );
	public Point3<T> ZXW => new( Z, X, W );
	public Point2<T> YW => new( Y, W );
	public Point3<T> XYW => new( X, Y, W );
	public Point3<T> ZYW => new( Z, Y, W );
	public Point2<T> ZW => new( Z, W );
	public Point3<T> XZW => new( X, Z, W );
	public Point3<T> YZW => new( Y, Z, W );
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref X, 4 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref X, 4 );
	
	public Point4<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			X = Y.CreateChecked( X ),
			Y = Y.CreateChecked( this.Y ),
			Z = Y.CreateChecked( Z ),
			W = Y.CreateChecked( W )
		};
	}
	
	public Vector4<T> FromOrigin () {
		return new() {
			X = X,
			Y = Y,
			Z = Z,
			W = W
		};
	}
	
	public Vector4<T> ToOrigin () {
		return new() {
			X = -X,
			Y = -Y,
			Z = -Z,
			W = -W
		};
	}
	
	public Point4<T> ScaleAboutOrigin ( T scale ) {
		return new() {
			X = X * scale,
			Y = Y * scale,
			Z = Z * scale,
			W = W * scale
		};
	}
	
	public Point4<T> ReflectAboutOrigin () {
		return new() {
			X = -X,
			Y = -Y,
			Z = -Z,
			W = -W
		};
	}
	
	public Point4<T> Lerp ( Point4<T> goal, T time ) {
		return new() {
			X = X.Lerp( goal.X, time ),
			Y = Y.Lerp( goal.Y, time ),
			Z = Z.Lerp( goal.Z, time ),
			W = W.Lerp( goal.W, time )
		};
	}
	
	public static Point4<T> operator + ( Point4<T> point, Vector4<T> delta ) {
		return new() {
			X = point.X + delta.X,
			Y = point.Y + delta.Y,
			Z = point.Z + delta.Z,
			W = point.W + delta.W
		};
	}
	
	public static Point4<T> operator - ( Point4<T> point, Vector4<T> delta ) {
		return new() {
			X = point.X - delta.X,
			Y = point.Y - delta.Y,
			Z = point.Z - delta.Z,
			W = point.W - delta.W
		};
	}
	
	public static Vector4<T> operator - ( Point4<T> to, Point4<T> from ) {
		return new() {
			X = to.X - from.X,
			Y = to.Y - from.Y,
			Z = to.Z - from.Z,
			W = to.W - from.W
		};
	}
	
	public static bool operator == ( Point4<T> left, Point4<T> right ) {
		return left.X == right.X
			&& left.Y == right.Y
			&& left.Z == right.Z
			&& left.W == right.W;
	}
	
	public static bool operator != ( Point4<T> left, Point4<T> right ) {
		return left.X != right.X
			|| left.Y != right.Y
			|| left.Z != right.Z
			|| left.W != right.W;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is Point4<T> axes && Equals( axes );
	}
	
	public bool Equals ( Point4<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( X, Y, Z, W );
	}
	
	public override string ToString () {
		return $"({X}, {Y}, {Z}, {W})";
	}
}
