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
		data.CopyTo( this.AsSpan2D() );
	}
#nullable restore

	public static readonly Matrix3<T> Identity = new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M22 = T.MultiplicativeIdentity
	};

	public static Matrix3<T> CreateScale ( Axes2<T> scale ) => CreateScale( scale.X, scale.Y );
	public static Matrix3<T> CreateScale ( T x, T y ) => new() {
		M00 = x,
		M11 = y,
		M22 = T.MultiplicativeIdentity
	};

	public static Matrix3<T> CreateShear ( Axes2<T> shear ) => CreateShear( shear.X, shear.Y );
	public static Matrix3<T> CreateShear ( T x, T y ) => new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M22 = T.MultiplicativeIdentity,

		M01 = x,
		M10 = y
	};

	public static Matrix3<T> CreateTranslation ( Vector2<T> transformation ) => CreateTranslation( transformation.X, transformation.Y );
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

	public static Matrix3<T> CreateViewport ( T x, T y, T width, T height ) {
		return CreateTranslation( -x, -y ) * CreateScale( T.MultiplicativeIdentity / width, T.MultiplicativeIdentity / height );
	}

	public static Matrix3<TNumber> CreateLookAt<TNumber> ( Vector2<TNumber> direction ) where TNumber : unmanaged, IFloatingPointIeee754<TNumber> {
		return Matrix3<TNumber>.CreateRotation( direction.GetAngle() );
	}

	public T Determinant {
		get {
			var m = AsSpan();
			return m[0] * m[4] * m[8] - m[0] * m[5] * m[7]
				 - m[1] * m[3] * m[8] + m[1] * m[5] * m[6]
				 + m[2] * m[3] * m[7] - m[2] * m[4] * m[6];
		}
	}

	public Matrix3<T> Inversed () {
		var m = AsSpan();
		var det = T.MultiplicativeIdentity / Determinant;

		return new Matrix3<T> {
			M00 = det * ( m[4] * m[8] - m[5] * m[7] ),
			M10 = det * ( m[2] * m[7] - m[1] * m[8] ),
			M20 = det * ( m[1] * m[5] - m[2] * m[4] ),
			M01 = det * ( m[5] * m[6] - m[3] * m[8] ),
			M11 = det * ( m[0] * m[8] - m[2] * m[6] ),
			M21 = det * ( m[2] * m[3] - m[0] * m[5] ),
			M02 = det * ( m[3] * m[7] - m[4] * m[6] ),
			M12 = det * ( m[1] * m[6] - m[0] * m[7] ),
			M22 = det * ( m[0] * m[4] - m[1] * m[3] )
		};
	}

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

	public static Vector3<T> operator * ( Vector3<T> vector, Matrix3<T> matrix ) {
		var M = matrix.AsSpan();

		return new() {
			X = vector.Z * M[6] + vector.X * M[0] + vector.Y * M[3],
			Y = vector.Z * M[7] + vector.X * M[1] + vector.Y * M[4],
			Z = vector.Z * M[8] + vector.X * M[2] + vector.Y * M[5]
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

	public Matrix4<T> ToMatrix4 () {
		return new Matrix4<T>( AsSpan2D() );
	}
}
