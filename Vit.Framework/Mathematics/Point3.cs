/// This file [Point3.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.PointTemplate and parameter 3 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics;

public struct Point3<T> : IInterpolatable<Point3<T>, T>, IEqualityOperators<Point3<T>, Point3<T>, bool>, IEquatable<Point3<T>>, IValueSpan<T> where T : INumber<T> {
	public T X;
	public T Y;
	public T Z;
	
	public Point3 ( T x, T y, T z ) {
		X = x;
		Y = y;
		Z = z;
	}
	
	public Point3 ( T all ) {
		X = Y = Z = all;
	}
	
	#nullable disable
	public Point3 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	
	public Point3 ( IReadOnlyValueSpan<T> span ) {
		span.AsReadOnlySpan().CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly Point3<T> UnitX = new( T.One, T.Zero, T.Zero );
	public static readonly Point3<T> UnitY = new( T.Zero, T.One, T.Zero );
	public static readonly Point3<T> UnitZ = new( T.Zero, T.Zero, T.One );
	public static readonly Point3<T> One = new( T.One );
	public static readonly Point3<T> Zero = new( T.Zero );
	
	public Point2<T> YX => new( Y, X );
	public Point2<T> ZX => new( Z, X );
	public Point2<T> XY => new( X, Y );
	public Point2<T> ZY => new( Z, Y );
	public Point2<T> XZ => new( X, Z );
	public Point2<T> YZ => new( Y, Z );
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref X, 3 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref X, 3 );
	
	public Point3<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			X = Y.CreateChecked( X ),
			Y = Y.CreateChecked( this.Y ),
			Z = Y.CreateChecked( Z )
		};
	}
	
	public Vector3<T> FromOrigin () {
		return new() {
			X = X,
			Y = Y,
			Z = Z
		};
	}
	
	public Vector3<T> ToOrigin () {
		return new() {
			X = -X,
			Y = -Y,
			Z = -Z
		};
	}
	
	public Point3<T> ScaleAboutOrigin ( T scale ) {
		return new() {
			X = X * scale,
			Y = Y * scale,
			Z = Z * scale
		};
	}
	
	public Point3<T> ReflectAboutOrigin () {
		return new() {
			X = -X,
			Y = -Y,
			Z = -Z
		};
	}
	
	public Point3<T> Lerp ( Point3<T> goal, T time ) {
		return new() {
			X = X.Lerp( goal.X, time ),
			Y = Y.Lerp( goal.Y, time ),
			Z = Z.Lerp( goal.Z, time )
		};
	}
	
	public static Point3<T> operator + ( Point3<T> point, Vector3<T> delta ) {
		return new() {
			X = point.X + delta.X,
			Y = point.Y + delta.Y,
			Z = point.Z + delta.Z
		};
	}
	
	public static Point3<T> operator - ( Point3<T> point, Vector3<T> delta ) {
		return new() {
			X = point.X - delta.X,
			Y = point.Y - delta.Y,
			Z = point.Z - delta.Z
		};
	}
	
	public static Vector3<T> operator - ( Point3<T> to, Point3<T> from ) {
		return new() {
			X = to.X - from.X,
			Y = to.Y - from.Y,
			Z = to.Z - from.Z
		};
	}
	
	public static bool operator == ( Point3<T> left, Point3<T> right ) {
		return left.X == right.X
			&& left.Y == right.Y
			&& left.Z == right.Z;
	}
	
	public static bool operator != ( Point3<T> left, Point3<T> right ) {
		return left.X != right.X
			|| left.Y != right.Y
			|| left.Z != right.Z;
	}
	public static implicit operator Point3<T> ( (T, T, T) value )
		=> new( value.Item1, value.Item2, value.Item3 );
	
	public void Deconstruct ( out T x, out T y, out T z ) {
		x = X;
		y = Y;
		z = Z;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is Point3<T> axes && Equals( axes );
	}
	
	public bool Equals ( Point3<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( X, Y, Z );
	}
	
	public override string ToString () {
		return $"({X}, {Y}, {Z})";
	}
}
