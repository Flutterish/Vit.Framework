/// This file [Matrix4x2.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.MatrixTemplate and parameter (2, 4) (System.ValueTuple`2[System.Int32,System.Int32])
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics.LinearAlgebra.Generic;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Matrix4x2<T> where T : INumber<T> {
	public T M00; public T M10; public T M20; public T M30;
	public T M01; public T M11; public T M21; public T M31;
	
	#nullable disable
	public Matrix4x2 ( ReadOnlySpan2D<T> data ) {
		data.CopyTo( this.AsSpan2D() );
	}
	#nullable restore
	
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref M00, 8 );
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref M00, 8 );
	public ReadOnlySpan2D<T> AsReadOnlySpan2D () => new( AsReadOnlySpan(), 4, 2 );
	public Span2D<T> AsSpan2D () => new( AsSpan(), 4, 2 );
	
	public Matrix<T> AsUnsized () => new Matrix<T>( AsSpan2D() );
	
	public static readonly Matrix4x2<T> Identity = new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity
	};
	
	public static Matrix4x2<T> CreateScale ( Axes2<T> axes )
		=> CreateScale( axes.X, axes.Y );
	public static Matrix4x2<T> CreateScale ( T x, T y ) => new() {
		M00 = x,
		M11 = y
	};
	public static Matrix4x2<T> CreateScale ( Axes1<T> axes )
		=> CreateScale( axes.X );
	public static Matrix4x2<T> CreateScale ( T x ) => new() {
		M00 = x,
		M11 = T.MultiplicativeIdentity
	};
	
	public static Matrix4x2<T> CreateTranslation ( Vector1<T> vector )
		=> CreateTranslation( vector.X );
	public static Matrix4x2<T> CreateTranslation ( T x ) => new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M01 = x
	};
	public static Matrix4x2<T> CreateShear ( Axes2<T> shear )
		=> CreateShear( shear.X, shear.Y );
	public static Matrix4x2<T> CreateShear ( T x, T y ) => new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		
		M01 = x,
		M10 = y
	};
	
	public static Matrix4x2<T> CreateRotation<TAngle> ( TAngle angle ) where TAngle : IAngle<TAngle, T> {
		T sin = TAngle.Sin( angle ), cos = TAngle.Cos( angle );
		
		return new() {
			M00 = cos,
			M01 = -sin,
			M10 = sin,
			M11 = cos,
		};
	}
	
	public static Matrix4x2<T> operator * ( Matrix4x2<T> left, Matrix4x2<T> right ) {
		var A = left.AsSpan();
		var B = right.AsSpan();
		
		return new() {
			M00 = A[0] * B[0] + A[1] * B[4],
			M10 = A[0] * B[1] + A[1] * B[5],
			M20 = A[0] * B[2] + A[1] * B[6],
			M30 = A[0] * B[3] + A[1] * B[7],
			M01 = A[4] * B[0] + A[5] * B[4],
			M11 = A[4] * B[1] + A[5] * B[5],
			M21 = A[4] * B[2] + A[5] * B[6],
			M31 = A[4] * B[3] + A[5] * B[7],
		};
	}
	
	public Vector4<T> Apply ( Vector4<T> value )
		=> value * this;
	public static Vector4<T> operator * ( Vector4<T> left, Matrix4x2<T> right ) {
		var M = right.AsSpan();
		var V = left.AsSpan();
		
		return new() {
			X = M[0] * V[0] + M[4] * V[1],
			Y = M[1] * V[0] + M[5] * V[1],
			Z = M[2] * V[0] + M[6] * V[1],
			W = M[3] * V[0] + M[7] * V[1],
		};
	}
	
	public Point4<T> Apply ( Point4<T> value )
		=> value * this;
	public static Point4<T> operator * ( Point4<T> left, Matrix4x2<T> right ) {
		var M = right.AsSpan();
		var V = left.AsSpan();
		
		return new() {
			X = M[0] * V[0] + M[4] * V[1],
			Y = M[1] * V[0] + M[5] * V[1],
			Z = M[2] * V[0] + M[6] * V[1],
			W = M[3] * V[0] + M[7] * V[1],
		};
	}
	
	public Vector3<T> Apply ( Vector3<T> value ) {
		var M = this.AsSpan();
		var V = value.AsSpan();
		
		return new() {
			X = M[0] * V[0] + M[4] * V[1],
			Y = M[1] * V[0] + M[5] * V[1],
			Z = M[2] * V[0] + M[6] * V[1],
		};
	}
	
	public Point3<T> Apply ( Point3<T> value ) {
		var M = this.AsSpan();
		var V = value.AsSpan();
		
		return new() {
			X = M[0] * V[0] + M[4] * V[1],
			Y = M[1] * V[0] + M[5] * V[1],
			Z = M[2] * V[0] + M[6] * V[1],
		};
	}
	
	public static implicit operator Span2D<T> ( Matrix4x2<T> matrix )
		=> matrix.AsSpan2D();
	public static implicit operator ReadOnlySpan2D<T> ( Matrix4x2<T> matrix )
		=> matrix.AsReadOnlySpan2D();
	
	public override string ToString () {
		return Matrix<T>.ToString( AsSpan2D() );
	}
}
