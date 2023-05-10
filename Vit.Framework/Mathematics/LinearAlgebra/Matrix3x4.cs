/// This file [Matrix3x4.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.MatrixTemplate and parameter (4, 3) (System.ValueTuple`2[System.Int32,System.Int32])
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics.LinearAlgebra.Generic;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Matrix3x4<T> where T : INumber<T> {
	public T M00; public T M10; public T M20;
	public T M01; public T M11; public T M21;
	public T M02; public T M12; public T M22;
	public T M03; public T M13; public T M23;
	
	#nullable disable
	public Matrix3x4 ( ReadOnlySpan2D<T> data ) {
		data.CopyTo( this.AsSpan2D() );
	}
	#nullable restore
	
	public Matrix3x4 (
		T m00, T m10, T m20, 
		T m01, T m11, T m21, 
		T m02, T m12, T m22, 
		T m03, T m13, T m23
	) {
		M00 = m00; M10 = m10; M20 = m20; 
		M01 = m01; M11 = m11; M21 = m21; 
		M02 = m02; M12 = m12; M22 = m22; 
		M03 = m03; M13 = m13; M23 = m23; 
	}
	
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref M00, 12 );
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref M00, 12 );
	public ReadOnlySpan2D<T> AsReadOnlySpan2D () => new( AsReadOnlySpan(), 3, 4 );
	public Span2D<T> AsSpan2D () => new( AsSpan(), 3, 4 );
	
	public Matrix<T> AsUnsized () => new Matrix<T>( AsSpan2D() );
	
	public static readonly Matrix3x4<T> Identity = new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M22 = T.MultiplicativeIdentity
	};
	
	public static Matrix3x4<T> CreateScale ( Axes3<T> axes )
		=> CreateScale( axes.X, axes.Y, axes.Z );
	public static Matrix3x4<T> CreateScale ( T x, T y, T z ) => new() {
		M00 = x,
		M11 = y,
		M22 = z
	};
	public static Matrix3x4<T> CreateScale ( Axes2<T> axes )
		=> CreateScale( axes.X, axes.Y );
	public static Matrix3x4<T> CreateScale ( T x, T y ) => new() {
		M00 = x,
		M11 = y,
		M22 = T.MultiplicativeIdentity
	};
	
	public static Matrix3x4<T> CreateTranslation ( Vector2<T> vector )
		=> CreateTranslation( vector.X, vector.Y );
	public static Matrix3x4<T> CreateTranslation ( T x, T y ) => new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M22 = T.MultiplicativeIdentity,
		M02 = x,
		M12 = y
	};
	
	public static Matrix3x4<T> CreateShear ( Axes2<T> shear )
		=> CreateShear( shear.X, shear.Y );
	public static Matrix3x4<T> CreateShear ( T x, T y ) => new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M22 = T.MultiplicativeIdentity,
		
		M01 = x,
		M10 = y
	};
	
	public static Matrix3x4<T> FromAxisAngle<TAngle> ( Vector3<T> axis, TAngle angle ) where TAngle : IAngle<TAngle, T> {
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
			M22 = zz + ca * ( T.One - zz )
		};
	}
	
	public static Matrix3x4<T> CreateRotation<TAngle> ( TAngle angle ) where TAngle : IAngle<TAngle, T> {
		T sin = TAngle.Sin( angle ), cos = TAngle.Cos( angle );
		
		return new() {
			M00 = cos,
			M01 = -sin,
			M10 = sin,
			M11 = cos,
			
			M22 = T.MultiplicativeIdentity
		};
	}
	
	public static Matrix3x4<T> CreateViewport ( T x, T y, T width, T height ) {
		return CreateScale( T.MultiplicativeIdentity / width, T.MultiplicativeIdentity / height ) * CreateTranslation( -x, -y );
	}
	
	public static Matrix3x4<TNumber> CreateLookAt<TNumber> ( Vector2<TNumber> direction ) where TNumber : IFloatingPointIeee754<TNumber> {
		return Matrix3x4<TNumber>.CreateRotation( direction.GetAngle() );
	}
	
	public Matrix4x3<T> Transposed => new() {
		M00 = M00,
		M10 = M01,
		M20 = M02,
		M30 = M03,
		M01 = M10,
		M11 = M11,
		M21 = M12,
		M31 = M13,
		M02 = M20,
		M12 = M21,
		M22 = M22,
		M32 = M23,
	};
	
	public Matrix3x4<T> CofactorCheckerboard {
		get {
			var M = AsReadOnlySpan();
			return new() {
				M00 = M[ 0],
				M01 = -M[ 3],
				M02 = M[ 6],
				M03 = -M[ 9],
				M10 = -M[ 1],
				M11 = M[ 4],
				M12 = -M[ 7],
				M13 = M[10],
				M20 = M[ 2],
				M21 = -M[ 5],
				M22 = M[ 8],
				M23 = -M[11],
			};
		}
	}
	
	public static Matrix3x4<T> operator * ( Matrix3x4<T> left, Matrix3x4<T> right ) {
		var A = left.AsSpan();
		var B = right.AsSpan();
		
		return new() {
			M00 = A[ 0] * B[ 0] + A[ 1] * B[ 3] + A[ 2] * B[ 6],
			M10 = A[ 0] * B[ 1] + A[ 1] * B[ 4] + A[ 2] * B[ 7],
			M20 = A[ 0] * B[ 2] + A[ 1] * B[ 5] + A[ 2] * B[ 8],
			M01 = A[ 3] * B[ 0] + A[ 4] * B[ 3] + A[ 5] * B[ 6],
			M11 = A[ 3] * B[ 1] + A[ 4] * B[ 4] + A[ 5] * B[ 7],
			M21 = A[ 3] * B[ 2] + A[ 4] * B[ 5] + A[ 5] * B[ 8],
			M02 = A[ 6] * B[ 0] + A[ 7] * B[ 3] + A[ 8] * B[ 6],
			M12 = A[ 6] * B[ 1] + A[ 7] * B[ 4] + A[ 8] * B[ 7],
			M22 = A[ 6] * B[ 2] + A[ 7] * B[ 5] + A[ 8] * B[ 8],
			M03 = A[ 9] * B[ 0] + A[10] * B[ 3] + A[11] * B[ 6],
			M13 = A[ 9] * B[ 1] + A[10] * B[ 4] + A[11] * B[ 7],
			M23 = A[ 9] * B[ 2] + A[10] * B[ 5] + A[11] * B[ 8],
		};
	}
	
	public Vector3<T> Apply ( Vector3<T> value )
		=> value * this;
	public static Vector3<T> operator * ( Vector3<T> left, Matrix3x4<T> right ) {
		var M = right.AsSpan();
		var V = left.AsSpan();
		
		return new() {
			X = M[ 0] * V[0] + M[ 3] * V[1] + M[ 6] * V[2],
			Y = M[ 1] * V[0] + M[ 4] * V[1] + M[ 7] * V[2],
			Z = M[ 2] * V[0] + M[ 5] * V[1] + M[ 8] * V[2],
		};
	}
	
	public Point3<T> Apply ( Point3<T> value )
		=> value * this;
	public static Point3<T> operator * ( Point3<T> left, Matrix3x4<T> right ) {
		var M = right.AsSpan();
		var V = left.AsSpan();
		
		return new() {
			X = M[ 0] * V[0] + M[ 3] * V[1] + M[ 6] * V[2],
			Y = M[ 1] * V[0] + M[ 4] * V[1] + M[ 7] * V[2],
			Z = M[ 2] * V[0] + M[ 5] * V[1] + M[ 8] * V[2],
		};
	}
	
	public Vector2<T> Apply ( Vector2<T> value ) {
		var M = this.AsSpan();
		var V = value.AsSpan();
		
		return new() {
			X = M[ 6] + M[ 0] * V[0] + M[ 3] * V[1],
			Y = M[ 7] + M[ 1] * V[0] + M[ 4] * V[1],
		};
	}
	
	public Point2<T> Apply ( Point2<T> value ) {
		var M = this.AsSpan();
		var V = value.AsSpan();
		
		return new() {
			X = M[ 6] + M[ 0] * V[0] + M[ 3] * V[1],
			Y = M[ 7] + M[ 1] * V[0] + M[ 4] * V[1],
		};
	}
	
	public static implicit operator Span2D<T> ( Matrix3x4<T> matrix )
		=> matrix.AsSpan2D();
	public static implicit operator ReadOnlySpan2D<T> ( Matrix3x4<T> matrix )
		=> matrix.AsReadOnlySpan2D();
	
	public override string ToString () {
		return Matrix<T>.ToString( AsSpan2D() );
	}
}
