/// This file [Matrix4.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.MatrixTemplate and parameter (4, 4) (System.ValueTuple`2[System.Int32,System.Int32])
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics.LinearAlgebra.Generic;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Matrix4<T> where T : INumber<T> {
	public T M00; public T M10; public T M20; public T M30;
	public T M01; public T M11; public T M21; public T M31;
	public T M02; public T M12; public T M22; public T M32;
	public T M03; public T M13; public T M23; public T M33;
	
	#nullable disable
	public Matrix4 ( ReadOnlySpan2D<T> data ) {
		data.CopyTo( this.AsSpan2D() );
	}
	#nullable restore
	
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref M00, 16 );
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref M00, 16 );
	public ReadOnlySpan2D<T> AsReadOnlySpan2D () => new( AsReadOnlySpan(), 4, 4 );
	public Span2D<T> AsSpan2D () => new( AsSpan(), 4, 4 );
	
	public Matrix<T> AsUnsized () => new Matrix<T>( AsSpan2D() );
	
	public static readonly Matrix4<T> Identity = new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M22 = T.MultiplicativeIdentity,
		M33 = T.MultiplicativeIdentity
	};
	
	public static Matrix4<T> CreateScale ( Axes4<T> axes )
		=> CreateScale( axes.X, axes.Y, axes.Z, axes.W );
	public static Matrix4<T> CreateScale ( T x, T y, T z, T w ) => new() {
		M00 = x,
		M11 = y,
		M22 = z,
		M33 = w
	};
	public static Matrix4<T> CreateScale ( Axes3<T> axes )
		=> CreateScale( axes.X, axes.Y, axes.Z );
	public static Matrix4<T> CreateScale ( T x, T y, T z ) => new() {
		M00 = x,
		M11 = y,
		M22 = z,
		M33 = T.MultiplicativeIdentity
	};
	
	public static Matrix4<T> CreateTranslation ( Vector3<T> vector )
		=> CreateTranslation( vector.X, vector.Y, vector.Z );
	public static Matrix4<T> CreateTranslation ( T x, T y, T z ) => new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M22 = T.MultiplicativeIdentity,
		M33 = T.MultiplicativeIdentity,
		M03 = x,
		M13 = y,
		M23 = z
	};
	
	public static Matrix4<T> FromAxisAngle<TAngle> ( Vector3<T> axis, TAngle angle ) where TAngle : IAngle<TAngle, T> {
		T x = axis.X, y = axis.Y, z = axis.Z;
		T sa = TAngle.Sin( angle ), ca = TAngle.Cos( angle );
		T xx = x * x, yy = y * y, zz = z * z;
		T xy = x * y, xz = x * z, yz = y * z;
		
		return new() {
			M00 = xx + ca * ( T.One - xx ),
			M10 = xy - ca * xy + sa * z,
			M20 = xz - ca * xz - sa * y,
			
			M01 = xy - ca * xy - sa * z,
			M11 = yy + ca * ( T.One - yy ),
			M21 = yz - ca * yz + sa * x,
			
			M02 = xz - ca * xz + sa * y,
			M12 = yz - ca * yz - sa * x,
			M22 = zz + ca * ( T.One - zz ),
			
			M33 = T.MultiplicativeIdentity
		};
	}
	
	public static Matrix4<T> CreatePerspective ( T width, T height, T nearPlaneDistance, T farPlaneDistance ) {
		var a = T.MultiplicativeIdentity / ( T.MultiplicativeIdentity - nearPlaneDistance / farPlaneDistance );
		var b = nearPlaneDistance * a;
		
		var aspectRatio = width / height;
		
		return new() {
			M00 = aspectRatio > T.MultiplicativeIdentity ? height / width : T.MultiplicativeIdentity,
			M11 = aspectRatio < T.MultiplicativeIdentity ? aspectRatio : T.MultiplicativeIdentity,
			
			M22 = a,
			M23 = -b,
			
			M32 = T.MultiplicativeIdentity
		};
	}
	
	public static Matrix4<TNumber> CreateLookAt<TNumber> ( Vector3<TNumber> direction, Vector3<TNumber> upDirection ) 
		where TNumber : IFloatingPointIeee754<TNumber> 
	{
		var forward = direction.Normalized();
		var right = upDirection.Cross( forward );
		if ( right.LengthSquared <= TNumber.Epsilon ) {
			if ( TNumber.Abs( upDirection.X ) < TNumber.Abs( upDirection.Y ) )
				upDirection.X += TNumber.One;
			else
				upDirection.Y += TNumber.One;
			
			right = upDirection.Cross( forward );
		}
		right.Normalize();
		var up = forward.Cross( right );
		
		return new() {
			M02 = forward.X,
			M12 = forward.Y,
			M22 = forward.Z,
			
			M01 = up.X,
			M11 = up.Y,
			M21 = up.Z,
			
			M00 = right.X,
			M10 = right.Y,
			M20 = right.Z,
			
			M33 = TNumber.MultiplicativeIdentity
		};
	}
	
	public T Determinant {
		get {
			var M = AsReadOnlySpan();
			return M[ 0] * M[ 5] * M[10] * M[15]
				- M[ 0] * M[ 5] * M[11] * M[14]
				- M[ 0] * M[ 6] * M[ 9] * M[15]
				+ M[ 0] * M[ 6] * M[11] * M[13]
				+ M[ 0] * M[ 7] * M[ 9] * M[14]
				- M[ 0] * M[ 7] * M[10] * M[13]
				- M[ 1] * M[ 4] * M[10] * M[15]
				+ M[ 1] * M[ 4] * M[11] * M[14]
				+ M[ 1] * M[ 6] * M[ 8] * M[15]
				- M[ 1] * M[ 6] * M[11] * M[12]
				- M[ 1] * M[ 7] * M[ 8] * M[14]
				+ M[ 1] * M[ 7] * M[10] * M[12]
				+ M[ 2] * M[ 4] * M[ 9] * M[15]
				- M[ 2] * M[ 4] * M[11] * M[13]
				- M[ 2] * M[ 5] * M[ 8] * M[15]
				+ M[ 2] * M[ 5] * M[11] * M[12]
				+ M[ 2] * M[ 7] * M[ 8] * M[13]
				- M[ 2] * M[ 7] * M[ 9] * M[12]
				- M[ 3] * M[ 4] * M[ 9] * M[14]
				+ M[ 3] * M[ 4] * M[10] * M[13]
				+ M[ 3] * M[ 5] * M[ 8] * M[14]
				- M[ 3] * M[ 5] * M[10] * M[12]
				- M[ 3] * M[ 6] * M[ 8] * M[13]
				+ M[ 3] * M[ 6] * M[ 9] * M[12];
		}
	}
	
	public Matrix4<T> Inversed {
		get {
			var M = AsReadOnlySpan();
			var invDet = T.MultiplicativeIdentity / Determinant;
			return new() {
				M00 = (M[ 5] * M[10] * M[15] - M[ 5] * M[11] * M[14] - M[ 6] * M[ 9] * M[15] + M[ 6] * M[11] * M[13] + M[ 7] * M[ 9] * M[14] - M[ 7] * M[10] * M[13]) * invDet,
				M10 = (-M[ 1] * M[10] * M[15] + M[ 1] * M[11] * M[14] + M[ 2] * M[ 9] * M[15] - M[ 2] * M[11] * M[13] - M[ 3] * M[ 9] * M[14] + M[ 3] * M[10] * M[13]) * invDet,
				M20 = (M[ 1] * M[ 6] * M[15] - M[ 1] * M[ 7] * M[14] - M[ 2] * M[ 5] * M[15] + M[ 2] * M[ 7] * M[13] + M[ 3] * M[ 5] * M[14] - M[ 3] * M[ 6] * M[13]) * invDet,
				M30 = (-M[ 1] * M[ 6] * M[11] + M[ 1] * M[ 7] * M[10] + M[ 2] * M[ 5] * M[11] - M[ 2] * M[ 7] * M[ 9] - M[ 3] * M[ 5] * M[10] + M[ 3] * M[ 6] * M[ 9]) * invDet,
				M01 = (-M[ 4] * M[10] * M[15] + M[ 4] * M[11] * M[14] + M[ 6] * M[ 8] * M[15] - M[ 6] * M[11] * M[12] - M[ 7] * M[ 8] * M[14] + M[ 7] * M[10] * M[12]) * invDet,
				M11 = (M[ 0] * M[10] * M[15] - M[ 0] * M[11] * M[14] - M[ 2] * M[ 8] * M[15] + M[ 2] * M[11] * M[12] + M[ 3] * M[ 8] * M[14] - M[ 3] * M[10] * M[12]) * invDet,
				M21 = (-M[ 0] * M[ 6] * M[15] + M[ 0] * M[ 7] * M[14] + M[ 2] * M[ 4] * M[15] - M[ 2] * M[ 7] * M[12] - M[ 3] * M[ 4] * M[14] + M[ 3] * M[ 6] * M[12]) * invDet,
				M31 = (M[ 0] * M[ 6] * M[11] - M[ 0] * M[ 7] * M[10] - M[ 2] * M[ 4] * M[11] + M[ 2] * M[ 7] * M[ 8] + M[ 3] * M[ 4] * M[10] - M[ 3] * M[ 6] * M[ 8]) * invDet,
				M02 = (M[ 4] * M[ 9] * M[15] - M[ 4] * M[11] * M[13] - M[ 5] * M[ 8] * M[15] + M[ 5] * M[11] * M[12] + M[ 7] * M[ 8] * M[13] - M[ 7] * M[ 9] * M[12]) * invDet,
				M12 = (-M[ 0] * M[ 9] * M[15] + M[ 0] * M[11] * M[13] + M[ 1] * M[ 8] * M[15] - M[ 1] * M[11] * M[12] - M[ 3] * M[ 8] * M[13] + M[ 3] * M[ 9] * M[12]) * invDet,
				M22 = (M[ 0] * M[ 5] * M[15] - M[ 0] * M[ 7] * M[13] - M[ 1] * M[ 4] * M[15] + M[ 1] * M[ 7] * M[12] + M[ 3] * M[ 4] * M[13] - M[ 3] * M[ 5] * M[12]) * invDet,
				M32 = (-M[ 0] * M[ 5] * M[11] + M[ 0] * M[ 7] * M[ 9] + M[ 1] * M[ 4] * M[11] - M[ 1] * M[ 7] * M[ 8] - M[ 3] * M[ 4] * M[ 9] + M[ 3] * M[ 5] * M[ 8]) * invDet,
				M03 = (-M[ 4] * M[ 9] * M[14] + M[ 4] * M[10] * M[13] + M[ 5] * M[ 8] * M[14] - M[ 5] * M[10] * M[12] - M[ 6] * M[ 8] * M[13] + M[ 6] * M[ 9] * M[12]) * invDet,
				M13 = (M[ 0] * M[ 9] * M[14] - M[ 0] * M[10] * M[13] - M[ 1] * M[ 8] * M[14] + M[ 1] * M[10] * M[12] + M[ 2] * M[ 8] * M[13] - M[ 2] * M[ 9] * M[12]) * invDet,
				M23 = (-M[ 0] * M[ 5] * M[14] + M[ 0] * M[ 6] * M[13] + M[ 1] * M[ 4] * M[14] - M[ 1] * M[ 6] * M[12] - M[ 2] * M[ 4] * M[13] + M[ 2] * M[ 5] * M[12]) * invDet,
				M33 = (M[ 0] * M[ 5] * M[10] - M[ 0] * M[ 6] * M[ 9] - M[ 1] * M[ 4] * M[10] + M[ 1] * M[ 6] * M[ 8] + M[ 2] * M[ 4] * M[ 9] - M[ 2] * M[ 5] * M[ 8]) * invDet,
			};
		}
	}
	
	public static Matrix4<T> operator * ( Matrix4<T> left, Matrix4<T> right ) {
		var A = left.AsSpan();
		var B = right.AsSpan();
		
		return new() {
			M00 = A[ 0] * B[ 0] + A[ 1] * B[ 4] + A[ 2] * B[ 8] + A[ 3] * B[12],
			M10 = A[ 0] * B[ 1] + A[ 1] * B[ 5] + A[ 2] * B[ 9] + A[ 3] * B[13],
			M20 = A[ 0] * B[ 2] + A[ 1] * B[ 6] + A[ 2] * B[10] + A[ 3] * B[14],
			M30 = A[ 0] * B[ 3] + A[ 1] * B[ 7] + A[ 2] * B[11] + A[ 3] * B[15],
			M01 = A[ 4] * B[ 0] + A[ 5] * B[ 4] + A[ 6] * B[ 8] + A[ 7] * B[12],
			M11 = A[ 4] * B[ 1] + A[ 5] * B[ 5] + A[ 6] * B[ 9] + A[ 7] * B[13],
			M21 = A[ 4] * B[ 2] + A[ 5] * B[ 6] + A[ 6] * B[10] + A[ 7] * B[14],
			M31 = A[ 4] * B[ 3] + A[ 5] * B[ 7] + A[ 6] * B[11] + A[ 7] * B[15],
			M02 = A[ 8] * B[ 0] + A[ 9] * B[ 4] + A[10] * B[ 8] + A[11] * B[12],
			M12 = A[ 8] * B[ 1] + A[ 9] * B[ 5] + A[10] * B[ 9] + A[11] * B[13],
			M22 = A[ 8] * B[ 2] + A[ 9] * B[ 6] + A[10] * B[10] + A[11] * B[14],
			M32 = A[ 8] * B[ 3] + A[ 9] * B[ 7] + A[10] * B[11] + A[11] * B[15],
			M03 = A[12] * B[ 0] + A[13] * B[ 4] + A[14] * B[ 8] + A[15] * B[12],
			M13 = A[12] * B[ 1] + A[13] * B[ 5] + A[14] * B[ 9] + A[15] * B[13],
			M23 = A[12] * B[ 2] + A[13] * B[ 6] + A[14] * B[10] + A[15] * B[14],
			M33 = A[12] * B[ 3] + A[13] * B[ 7] + A[14] * B[11] + A[15] * B[15],
		};
	}
	
	public Vector4<T> Apply ( Vector4<T> value )
		=> value * this;
	public static Vector4<T> operator * ( Vector4<T> left, Matrix4<T> right ) {
		var M = right.AsSpan();
		var V = left.AsSpan();
		
		return new() {
			X = M[ 0] * V[0] + M[ 4] * V[1] + M[ 8] * V[2] + M[12] * V[3],
			Y = M[ 1] * V[0] + M[ 5] * V[1] + M[ 9] * V[2] + M[13] * V[3],
			Z = M[ 2] * V[0] + M[ 6] * V[1] + M[10] * V[2] + M[14] * V[3],
			W = M[ 3] * V[0] + M[ 7] * V[1] + M[11] * V[2] + M[15] * V[3],
		};
	}
	
	public Point4<T> Apply ( Point4<T> value )
		=> value * this;
	public static Point4<T> operator * ( Point4<T> left, Matrix4<T> right ) {
		var M = right.AsSpan();
		var V = left.AsSpan();
		
		return new() {
			X = M[ 0] * V[0] + M[ 4] * V[1] + M[ 8] * V[2] + M[12] * V[3],
			Y = M[ 1] * V[0] + M[ 5] * V[1] + M[ 9] * V[2] + M[13] * V[3],
			Z = M[ 2] * V[0] + M[ 6] * V[1] + M[10] * V[2] + M[14] * V[3],
			W = M[ 3] * V[0] + M[ 7] * V[1] + M[11] * V[2] + M[15] * V[3],
		};
	}
	
	public Vector3<T> Apply ( Vector3<T> value ) {
		var M = this.AsSpan();
		var V = value.AsSpan();
		
		return new() {
			X = M[12] + M[ 0] * V[0] + M[ 4] * V[1] + M[ 8] * V[2],
			Y = M[13] + M[ 1] * V[0] + M[ 5] * V[1] + M[ 9] * V[2],
			Z = M[14] + M[ 2] * V[0] + M[ 6] * V[1] + M[10] * V[2],
		};
	}
	
	public Point3<T> Apply ( Point3<T> value ) {
		var M = this.AsSpan();
		var V = value.AsSpan();
		
		return new() {
			X = M[12] + M[ 0] * V[0] + M[ 4] * V[1] + M[ 8] * V[2],
			Y = M[13] + M[ 1] * V[0] + M[ 5] * V[1] + M[ 9] * V[2],
			Z = M[14] + M[ 2] * V[0] + M[ 6] * V[1] + M[10] * V[2],
		};
	}
	
	public static implicit operator Span2D<T> ( Matrix4<T> matrix )
		=> matrix.AsSpan2D();
	public static implicit operator ReadOnlySpan2D<T> ( Matrix4<T> matrix )
		=> matrix.AsReadOnlySpan2D();
	
	public override string ToString () {
		return Matrix<T>.ToString( AsSpan2D() );
	}
}
