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
		where TNumber : unmanaged, IFloatingPointIeee754<TNumber> 
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
			T a = M00, b = M01, c = M02, d = M03;
			T e = M10, f = M11, g = M12, h = M13;
			T i = M20, j = M21, k = M22, l = M23;
			T m = M30, n = M31, o = M32, p = M33;

			var kp_lo = k * p - l * o;
			var jp_ln = j * p - l * n;
			var jo_kn = j * o - k * n;
			var ip_lm = i * p - l * m;
			var io_km = i * o - k * m;
			var in_jm = i * n - j * m;

			return a * ( f * kp_lo - g * jp_ln + h * jo_kn ) -
				   b * ( e * kp_lo - g * ip_lm + h * io_km ) +
				   c * ( e * jp_ln - f * ip_lm + h * in_jm ) -
				   d * ( e * jo_kn - f * io_km + g * in_jm );
		}
	}

	public Matrix4<T> Inverse () {
		T a = M00, b = M01, c = M02, d = M03;
		T e = M10, f = M11, g = M12, h = M13;
		T i = M20, j = M21, k = M22, l = M23;
		T m = M30, n = M31, o = M32, p = M33;

		var kp_lo = k * p - l * o;
		var jp_ln = j * p - l * n;
		var jo_kn = j * o - k * n;
		var ip_lm = i * p - l * m;
		var io_km = i * o - k * m;
		var in_jm = i * n - j * m;

		var a11 = +( f * kp_lo - g * jp_ln + h * jo_kn );
		var a12 = -( e * kp_lo - g * ip_lm + h * io_km );
		var a13 = +( e * jp_ln - f * ip_lm + h * in_jm );
		var a14 = -( e * jo_kn - f * io_km + g * in_jm );

		var det = a * a11 + b * a12 + c * a13 + d * a14;
		var invDet = T.MultiplicativeIdentity / det;

		var gp_ho = g * p - h * o;
		var fp_hn = f * p - h * n;
		var fo_gn = f * o - g * n;
		var ep_hm = e * p - h * m;
		var eo_gm = e * o - g * m;
		var en_fm = e * n - f * m;

		var gl_hk = g * l - h * k;
		var fl_hj = f * l - h * j;
		var fk_gj = f * k - g * j;
		var el_hi = e * l - h * i;
		var ek_gi = e * k - g * i;
		var ej_fi = e * j - f * i;

		return new() {
			M00 = a11 * invDet,
			M10 = a12 * invDet,
			M20 = a13 * invDet,
			M30 = a14 * invDet,
			M01 = -( b * kp_lo - c * jp_ln + d * jo_kn ) * invDet,
			M11 = +( a * kp_lo - c * ip_lm + d * io_km ) * invDet,
			M21 = -( a * jp_ln - b * ip_lm + d * in_jm ) * invDet,
			M31 = +( a * jo_kn - b * io_km + c * in_jm ) * invDet,
			M02 = +( b * gp_ho - c * fp_hn + d * fo_gn ) * invDet,
			M12 = -( a * gp_ho - c * ep_hm + d * eo_gm ) * invDet,
			M22 = +( a * fp_hn - b * ep_hm + d * en_fm ) * invDet,
			M32 = -( a * fo_gn - b * eo_gm + c * en_fm ) * invDet,
			M03 = -( b * gl_hk - c * fl_hj + d * fk_gj ) * invDet,
			M13 = +( a * gl_hk - c * el_hi + d * ek_gi ) * invDet,
			M23 = -( a * fl_hj - b * el_hi + d * ej_fi ) * invDet,
			M33 = +( a * fk_gj - b * ek_gi + c * ej_fi ) * invDet
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
