using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics.LinearAlgebra.Generic;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Matrix3<T> where T : INumber<T> {
	public T M00; public T M10; public T M20;
	public T M01; public T M11; public T M21;
	public T M02; public T M12; public T M22;

	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref M00, 9 );
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref M00, 9 );
	public ReadOnlySpan2D<T> AsReadonlySpan2D () => new( AsReadOnlySpan(), 3, 3 );
	public Span2D<T> AsSpan2D () => new( AsSpan(), 3, 3 );

	public Matrix<T> AsUnsized () => new Matrix<T>( AsSpan2D() );

#nullable disable
	public Matrix3 ( ReadOnlySpan2D<T> data ) {
		data.Flat.CopyTo( this.AsSpan() );
	}
#nullable restore

	public static readonly Matrix3<T> Identity = new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M22 = T.MultiplicativeIdentity
	};

	public static Matrix3<T> CreateScale ( T x, T y ) => new() {
		M00 = x,
		M11 = y,
		M22 = T.MultiplicativeIdentity
	};

	public static Matrix3<T> CreateTranslation ( T x, T y ) => new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M22 = T.MultiplicativeIdentity,

		M02 = x,
		M12 = y
	};

	public static Matrix3<T> CreateRotation<TAngle> ( TAngle angle ) where TAngle : IAngle<TAngle, T> {
		T sin = TAngle.Sin( angle ), cos = TAngle.Cos( angle );

		return new() {
			M00 = cos,
			M01 = -sin,
			M10 = sin,
			M11 = cos,

			M22 = T.MultiplicativeIdentity
		};
	}

	//public static Matrix3<T> CreateViewport ( T x, T y, T width, T height ) {
	//	return CreateTranslation( -x, -y ) * CreateScale( T.MultiplicativeIdentity / width, T.MultiplicativeIdentity / height );
	//}

	public static Matrix3<TNumber> CreateLookAt<TNumber> ( Vector2<TNumber> direction ) where TNumber : unmanaged, IFloatingPointIeee754<TNumber> {
		return Matrix3<TNumber>.CreateRotation( direction.GetAngle() );
	}

	//public T Determinant {
	//	get {
	//		T a = M00, b = M01, c = M02, d = M03;
	//		T e = M10, f = M11, g = M12, h = M13;
	//		T i = M20, j = M21, k = M22, l = M23;
	//		T m = M30, n = M31, o = M32, p = M33;

	//		var kp_lo = k * p - l * o;
	//		var jp_ln = j * p - l * n;
	//		var jo_kn = j * o - k * n;
	//		var ip_lm = i * p - l * m;
	//		var io_km = i * o - k * m;
	//		var in_jm = i * n - j * m;

	//		return a * ( f * kp_lo - g * jp_ln + h * jo_kn ) -
	//			   b * ( e * kp_lo - g * ip_lm + h * io_km ) +
	//			   c * ( e * jp_ln - f * ip_lm + h * in_jm ) -
	//			   d * ( e * jo_kn - f * io_km + g * in_jm );
	//	}
	//}

	//public Matrix3<T> Inverse () {
	//	T a = M00, b = M01, c = M02, d = M03;
	//	T e = M10, f = M11, g = M12, h = M13;
	//	T i = M20, j = M21, k = M22, l = M23;
	//	T m = M30, n = M31, o = M32, p = M33;

	//	var kp_lo = k * p - l * o;
	//	var jp_ln = j * p - l * n;
	//	var jo_kn = j * o - k * n;
	//	var ip_lm = i * p - l * m;
	//	var io_km = i * o - k * m;
	//	var in_jm = i * n - j * m;

	//	var a11 = +( f * kp_lo - g * jp_ln + h * jo_kn );
	//	var a12 = -( e * kp_lo - g * ip_lm + h * io_km );
	//	var a13 = +( e * jp_ln - f * ip_lm + h * in_jm );
	//	var a14 = -( e * jo_kn - f * io_km + g * in_jm );

	//	var det = a * a11 + b * a12 + c * a13 + d * a14;
	//	var invDet = T.MultiplicativeIdentity / det;

	//	var gp_ho = g * p - h * o;
	//	var fp_hn = f * p - h * n;
	//	var fo_gn = f * o - g * n;
	//	var ep_hm = e * p - h * m;
	//	var eo_gm = e * o - g * m;
	//	var en_fm = e * n - f * m;

	//	var gl_hk = g * l - h * k;
	//	var fl_hj = f * l - h * j;
	//	var fk_gj = f * k - g * j;
	//	var el_hi = e * l - h * i;
	//	var ek_gi = e * k - g * i;
	//	var ej_fi = e * j - f * i;

	//	return new() {
	//		M00 = a11 * invDet,
	//		M10 = a12 * invDet,
	//		M20 = a13 * invDet,
	//		M30 = a14 * invDet,
	//		M01 = -( b * kp_lo - c * jp_ln + d * jo_kn ) * invDet,
	//		M11 = +( a * kp_lo - c * ip_lm + d * io_km ) * invDet,
	//		M21 = -( a * jp_ln - b * ip_lm + d * in_jm ) * invDet,
	//		M31 = +( a * jo_kn - b * io_km + c * in_jm ) * invDet,
	//		M02 = +( b * gp_ho - c * fp_hn + d * fo_gn ) * invDet,
	//		M12 = -( a * gp_ho - c * ep_hm + d * eo_gm ) * invDet,
	//		M22 = +( a * fp_hn - b * ep_hm + d * en_fm ) * invDet,
	//		M32 = -( a * fo_gn - b * eo_gm + c * en_fm ) * invDet,
	//		M03 = -( b * gl_hk - c * fl_hj + d * fk_gj ) * invDet,
	//		M13 = +( a * gl_hk - c * el_hi + d * ek_gi ) * invDet,
	//		M23 = -( a * fl_hj - b * el_hi + d * ej_fi ) * invDet,
	//		M33 = +( a * fk_gj - b * ek_gi + c * ej_fi ) * invDet
	//	};
	//}

	public static Matrix3<T> operator * ( Matrix3<T> left, Matrix3<T> right ) {
		var A = left.AsSpan();
		var B = right.AsSpan();

		return new() {
			M00 = A[0] * B[0] + A[1] * B[3] + A[2] * B[6],
			M10 = A[0] * B[1] + A[1] * B[4] + A[2] * B[7],
			M20 = A[0] * B[2] + A[1] * B[5] + A[2] * B[8],
			M01 = A[3] * B[0] + A[4] * B[3] + A[5] * B[6],
			M11 = A[3] * B[1] + A[4] * B[4] + A[5] * B[7],
			M21 = A[3] * B[2] + A[4] * B[5] + A[5] * B[8],
			M02 = A[6] * B[0] + A[7] * B[3] + A[8] * B[6],
			M12 = A[6] * B[1] + A[7] * B[4] + A[8] * B[7],
			M22 = A[6] * B[2] + A[7] * B[5] + A[8] * B[8]
		};
	}

	public Vector2<T> Apply ( Vector2<T> vector ) {
		return new() {
			X = M02 + vector.X * M00 + vector.Y * M01,
			Y = M12 + vector.X * M10 + vector.Y * M11
		};
	}

	public override string ToString () {
		return Matrix<T>.ToString( AsSpan2D() );
	}
}
