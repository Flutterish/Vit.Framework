using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics.LinearAlgebra.Generic;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Matrix4<T> where T : unmanaged, INumber<T> {
	public T M00; public T M10; public T M20; public T M30;
	public T M01; public T M11; public T M21; public T M31;
	public T M02; public T M12; public T M22; public T M32;
	public T M03; public T M13; public T M23; public T M33;

	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref M00, 16 );
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref M00, 16 );
	public ReadOnlySpan2D<T> AsReadonlySpan2D () => new( AsReadOnlySpan(), 4, 4 );
	public Span2D<T> AsSpan2D () => new( AsSpan(), 4, 4 );

	public Matrix<T> AsUnsized () => new Matrix<T>( AsSpan2D() );

	public Matrix4 ( ReadOnlySpan2D<T> data ) {
		data.Flat.CopyTo( this.AsSpan() );
	}

	public static readonly Matrix4<T> Identity = new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M22 = T.MultiplicativeIdentity,
		M33 = T.MultiplicativeIdentity
	};

	public static Matrix4<T> CreateScale ( T x, T y, T z ) => new() {
		M00 = x,
		M11 = y,
		M22 = z,
		M33 = T.MultiplicativeIdentity
	};

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
			M01 = xy - ca * xy + sa * z,
			M02 = xz - ca * xz - sa * y,

			M10 = xy - ca * xy - sa * z,
			M11 = yy + ca * ( T.One - yy ),
			M12 = yz - ca * yz + sa * x,

			M20 = xz - ca * xz + sa * y,
			M21 = yz - ca * yz - sa * x,
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

	public static Matrix4<TNumber> CreateLookAt<TNumber> ( Vector3<TNumber> cameraPosition, Vector3<TNumber> cameraTarget, Vector3<TNumber> cameraUpVector ) 
		where TNumber : unmanaged, IFloatingPointIeee754<TNumber> 
	{
		var forward = (cameraPosition - cameraTarget).Normalized();
		var right = cameraUpVector.Cross( forward );
		if ( right.LengthSquared <= TNumber.Epsilon ) {
			if ( TNumber.Abs( cameraUpVector.X ) < TNumber.Abs( cameraUpVector.Y ) )
				cameraUpVector.X += TNumber.One;
			else
				cameraUpVector.Y += TNumber.One;

			right = cameraUpVector.Cross( forward );
		}
		right.Normalize();
		var up = forward.Cross( right );

		return new() {
			M00 = right.X,
			M10 = up.X,
			M20 = forward.X,
			M01 = right.Y,
			M11 = up.Y,
			M21 = forward.Y,
			M02 = right.Z,
			M12 = up.Z,
			M22 = forward.Z,
			M03 = -right.Dot( cameraPosition ),
			M13 = -up.Dot( cameraPosition ),
			M23 = -forward.Dot( cameraPosition ),
			M33 = TNumber.MultiplicativeIdentity
		};
	}

	public static Matrix4<T> operator * ( Matrix4<T> left, Matrix4<T> right ) {
		var A = left.AsSpan();
		var B = right.AsSpan();

		return new() {
			M00 = A[ 0] * B[0] + A[ 1] * B[4] + A[ 2] * B[ 8] + A[ 3] * B[12],
			M10 = A[ 0] * B[1] + A[ 1] * B[5] + A[ 2] * B[ 9] + A[ 3] * B[13],
			M20 = A[ 0] * B[2] + A[ 1] * B[6] + A[ 2] * B[10] + A[ 3] * B[14],
			M30 = A[ 0] * B[3] + A[ 1] * B[7] + A[ 2] * B[11] + A[ 3] * B[15],
			M01 = A[ 4] * B[0] + A[ 5] * B[4] + A[ 6] * B[ 8] + A[ 7] * B[12],
			M11 = A[ 4] * B[1] + A[ 5] * B[5] + A[ 6] * B[ 9] + A[ 7] * B[13],
			M21 = A[ 4] * B[2] + A[ 5] * B[6] + A[ 6] * B[10] + A[ 7] * B[14],
			M31 = A[ 4] * B[3] + A[ 5] * B[7] + A[ 6] * B[11] + A[ 7] * B[15],
			M02 = A[ 8] * B[0] + A[ 9] * B[4] + A[10] * B[ 8] + A[11] * B[12],
			M12 = A[ 8] * B[1] + A[ 9] * B[5] + A[10] * B[ 9] + A[11] * B[13],
			M22 = A[ 8] * B[2] + A[ 9] * B[6] + A[10] * B[10] + A[11] * B[14],
			M32 = A[ 8] * B[3] + A[ 9] * B[7] + A[10] * B[11] + A[11] * B[15],
			M03 = A[12] * B[0] + A[13] * B[4] + A[14] * B[ 8] + A[15] * B[12],
			M13 = A[12] * B[1] + A[13] * B[5] + A[14] * B[ 9] + A[15] * B[13],
			M23 = A[12] * B[2] + A[13] * B[6] + A[14] * B[10] + A[15] * B[14],
			M33 = A[12] * B[3] + A[13] * B[7] + A[14] * B[11] + A[15] * B[15]
		};
	}

	public Vector3<T> Apply ( Vector3<T> vector ) {
		return new() {
			X = M03 + vector.X * M00 + vector.Y * M01 + vector.Z * M02,
			Y = M13 + vector.X * M10 + vector.Y * M11 + vector.Z * M12,
			Z = M23 + vector.X * M20 + vector.Y * M21 + vector.Z * M22
		};
	}

	public override string ToString () {
		return Matrix<T>.ToString( AsSpan2D() );
	}
}
