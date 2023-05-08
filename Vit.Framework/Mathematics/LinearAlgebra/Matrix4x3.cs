/// This file [Matrix4x3.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.MatrixTemplate and parameter (3, 4) (System.ValueTuple`2[System.Int32,System.Int32])
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics.LinearAlgebra.Generic;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Matrix4x3<T> where T : INumber<T> {
	public T M00; public T M10; public T M20; public T M30;
	public T M01; public T M11; public T M21; public T M31;
	public T M02; public T M12; public T M22; public T M32;
	
	#nullable disable
	public Matrix4x3 ( ReadOnlySpan2D<T> data ) {
		data.CopyTo( this.AsSpan2D() );
	}
	#nullable restore
	
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref M00, 12 );
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref M00, 12 );
	public ReadOnlySpan2D<T> AsReadOnlySpan2D () => new( AsReadOnlySpan(), 4, 3 );
	public Span2D<T> AsSpan2D () => new( AsSpan(), 4, 3 );
	
	public Matrix<T> AsUnsized () => new Matrix<T>( AsSpan2D() );
	
	public static readonly Matrix4x3<T> Identity = new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M22 = T.MultiplicativeIdentity
	};
	
	public static Matrix4x3<T> CreateScale ( Axes3<T> axes )
		=> CreateScale( axes.X, axes.Y, axes.Z );
	public static Matrix4x3<T> CreateScale ( T x, T y, T z ) => new() {
		M00 = x,
		M11 = y,
		M22 = z
	};
	public static Matrix4x3<T> CreateScale ( Axes2<T> axes )
		=> CreateScale( axes.X, axes.Y );
	public static Matrix4x3<T> CreateScale ( T x, T y ) => new() {
		M00 = x,
		M11 = y,
		M22 = T.MultiplicativeIdentity
	};
	
	public static Matrix4x3<T> CreateTranslation ( Vector2<T> vector )
		=> CreateTranslation( vector.X, vector.Y );
	public static Matrix4x3<T> CreateTranslation ( T x, T y ) => new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M22 = T.MultiplicativeIdentity,
		M02 = x,
		M12 = y
	};
	public static Matrix4x3<T> CreateShear ( Axes2<T> shear )
		=> CreateShear( shear.X, shear.Y );
	public static Matrix4x3<T> CreateShear ( T x, T y ) => new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M22 = T.MultiplicativeIdentity,
		
		M01 = x,
		M10 = y
	};
	
	public static Matrix4x3<T> FromAxisAngle<TAngle> ( Vector3<T> axis, TAngle angle ) where TAngle : IAngle<TAngle, T> {
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
	
	public static Matrix4x3<T> CreateRotation<TAngle> ( TAngle angle ) where TAngle : IAngle<TAngle, T> {
		T sin = TAngle.Sin( angle ), cos = TAngle.Cos( angle );
		
		return new() {
			M00 = cos,
			M01 = -sin,
			M10 = sin,
			M11 = cos,
			
			M22 = T.MultiplicativeIdentity
		};
	}
	
	public static Matrix4x3<T> CreateViewport ( T x, T y, T width, T height ) {
		return CreateTranslation( -x, -y ) * CreateScale( T.MultiplicativeIdentity / width, T.MultiplicativeIdentity / height );
	}
	
	public static Matrix4x3<TNumber> CreateLookAt<TNumber> ( Vector2<TNumber> direction ) where TNumber : IFloatingPointIeee754<TNumber> {
		return Matrix4x3<TNumber>.CreateRotation( direction.GetAngle() );
	}
	
	public static Matrix4x3<T> operator * ( Matrix4x3<T> left, Matrix4x3<T> right ) {
		var A = left.AsSpan();
		var B = right.AsSpan();
		
		return new() {
			M00 = A[ 0] * B[ 0] + A[ 1] * B[ 4] + A[ 2] * B[ 8],
			M10 = A[ 0] * B[ 1] + A[ 1] * B[ 5] + A[ 2] * B[ 9],
			M20 = A[ 0] * B[ 2] + A[ 1] * B[ 6] + A[ 2] * B[10],
			M30 = A[ 0] * B[ 3] + A[ 1] * B[ 7] + A[ 2] * B[11],
			M01 = A[ 4] * B[ 0] + A[ 5] * B[ 4] + A[ 6] * B[ 8],
			M11 = A[ 4] * B[ 1] + A[ 5] * B[ 5] + A[ 6] * B[ 9],
			M21 = A[ 4] * B[ 2] + A[ 5] * B[ 6] + A[ 6] * B[10],
			M31 = A[ 4] * B[ 3] + A[ 5] * B[ 7] + A[ 6] * B[11],
			M02 = A[ 8] * B[ 0] + A[ 9] * B[ 4] + A[10] * B[ 8],
			M12 = A[ 8] * B[ 1] + A[ 9] * B[ 5] + A[10] * B[ 9],
			M22 = A[ 8] * B[ 2] + A[ 9] * B[ 6] + A[10] * B[10],
			M32 = A[ 8] * B[ 3] + A[ 9] * B[ 7] + A[10] * B[11],
		};
	}
	
	public Vector4<T> Apply ( Vector4<T> value )
		=> value * this;
	public static Vector4<T> operator * ( Vector4<T> left, Matrix4x3<T> right ) {
		var M = right.AsSpan();
		var V = left.AsSpan();
		
		return new() {
			X = M[ 0] * V[0] + M[ 4] * V[1] + M[ 8] * V[2],
			Y = M[ 1] * V[0] + M[ 5] * V[1] + M[ 9] * V[2],
			Z = M[ 2] * V[0] + M[ 6] * V[1] + M[10] * V[2],
			W = M[ 3] * V[0] + M[ 7] * V[1] + M[11] * V[2],
		};
	}
	
	public Point4<T> Apply ( Point4<T> value )
		=> value * this;
	public static Point4<T> operator * ( Point4<T> left, Matrix4x3<T> right ) {
		var M = right.AsSpan();
		var V = left.AsSpan();
		
		return new() {
			X = M[ 0] * V[0] + M[ 4] * V[1] + M[ 8] * V[2],
			Y = M[ 1] * V[0] + M[ 5] * V[1] + M[ 9] * V[2],
			Z = M[ 2] * V[0] + M[ 6] * V[1] + M[10] * V[2],
			W = M[ 3] * V[0] + M[ 7] * V[1] + M[11] * V[2],
		};
	}
	
	public Vector3<T> Apply ( Vector3<T> value ) {
		var M = this.AsSpan();
		var V = value.AsSpan();
		
		return new() {
			X = M[ 0] * V[0] + M[ 4] * V[1] + M[ 8] * V[2],
			Y = M[ 1] * V[0] + M[ 5] * V[1] + M[ 9] * V[2],
			Z = M[ 2] * V[0] + M[ 6] * V[1] + M[10] * V[2],
		};
	}
	
	public Point3<T> Apply ( Point3<T> value ) {
		var M = this.AsSpan();
		var V = value.AsSpan();
		
		return new() {
			X = M[ 0] * V[0] + M[ 4] * V[1] + M[ 8] * V[2],
			Y = M[ 1] * V[0] + M[ 5] * V[1] + M[ 9] * V[2],
			Z = M[ 2] * V[0] + M[ 6] * V[1] + M[10] * V[2],
		};
	}
	
	public static implicit operator Span2D<T> ( Matrix4x3<T> matrix )
		=> matrix.AsSpan2D();
	public static implicit operator ReadOnlySpan2D<T> ( Matrix4x3<T> matrix )
		=> matrix.AsReadOnlySpan2D();
	
	public override string ToString () {
		return Matrix<T>.ToString( AsSpan2D() );
	}
}
