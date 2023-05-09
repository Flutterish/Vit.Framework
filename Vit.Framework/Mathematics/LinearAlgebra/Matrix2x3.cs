/// This file [Matrix2x3.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.MatrixTemplate and parameter (3, 2) (System.ValueTuple`2[System.Int32,System.Int32])
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics.LinearAlgebra.Generic;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Matrix2x3<T> where T : INumber<T> {
	public T M00; public T M10;
	public T M01; public T M11;
	public T M02; public T M12;
	
	#nullable disable
	public Matrix2x3 ( ReadOnlySpan2D<T> data ) {
		data.CopyTo( this.AsSpan2D() );
	}
	#nullable restore
	
	public Matrix2x3 (
		T m00, T m10, 
		T m01, T m11, 
		T m02, T m12
	) {
		M00 = m00; M10 = m10; 
		M01 = m01; M11 = m11; 
		M02 = m02; M12 = m12; 
	}
	
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref M00, 6 );
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref M00, 6 );
	public ReadOnlySpan2D<T> AsReadOnlySpan2D () => new( AsReadOnlySpan(), 2, 3 );
	public Span2D<T> AsSpan2D () => new( AsSpan(), 2, 3 );
	
	public Matrix<T> AsUnsized () => new Matrix<T>( AsSpan2D() );
	
	public static readonly Matrix2x3<T> Identity = new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity
	};
	
	public static Matrix2x3<T> CreateScale ( Axes2<T> axes )
		=> CreateScale( axes.X, axes.Y );
	public static Matrix2x3<T> CreateScale ( T x, T y ) => new() {
		M00 = x,
		M11 = y
	};
	public static Matrix2x3<T> CreateScale ( Axes1<T> axes )
		=> CreateScale( axes.X );
	public static Matrix2x3<T> CreateScale ( T x ) => new() {
		M00 = x,
		M11 = T.MultiplicativeIdentity
	};
	
	public static Matrix2x3<T> CreateTranslation ( Vector1<T> vector )
		=> CreateTranslation( vector.X );
	public static Matrix2x3<T> CreateTranslation ( T x ) => new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M01 = x
	};
	
	public static Matrix2x3<T> CreateShear ( Axes2<T> shear )
		=> CreateShear( shear.X, shear.Y );
	public static Matrix2x3<T> CreateShear ( T x, T y ) => new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		
		M01 = x,
		M10 = y
	};
	
	public static Matrix2x3<T> CreateRotation<TAngle> ( TAngle angle ) where TAngle : IAngle<TAngle, T> {
		T sin = TAngle.Sin( angle ), cos = TAngle.Cos( angle );
		
		return new() {
			M00 = cos,
			M01 = -sin,
			M10 = sin,
			M11 = cos,
		};
	}
	
	public Matrix3x2<T> Transposed => new() {
		M00 = M00,
		M10 = M01,
		M20 = M02,
		M01 = M10,
		M11 = M11,
		M21 = M12,
	};
	
	public Matrix2x3<T> CofactorCheckerboard {
		get {
			var M = AsReadOnlySpan();
			return new() {
				M00 = M[0],
				M01 = -M[2],
				M02 = M[4],
				M10 = -M[1],
				M11 = M[3],
				M12 = -M[5],
			};
		}
	}
	
	public static Matrix2x3<T> operator * ( Matrix2x3<T> left, Matrix2x3<T> right ) {
		var A = left.AsSpan();
		var B = right.AsSpan();
		
		return new() {
			M00 = A[0] * B[0] + A[1] * B[2],
			M10 = A[0] * B[1] + A[1] * B[3],
			M01 = A[2] * B[0] + A[3] * B[2],
			M11 = A[2] * B[1] + A[3] * B[3],
			M02 = A[4] * B[0] + A[5] * B[2],
			M12 = A[4] * B[1] + A[5] * B[3],
		};
	}
	
	public Vector2<T> Apply ( Vector2<T> value )
		=> value * this;
	public static Vector2<T> operator * ( Vector2<T> left, Matrix2x3<T> right ) {
		var M = right.AsSpan();
		var V = left.AsSpan();
		
		return new() {
			X = M[0] * V[0] + M[2] * V[1],
			Y = M[1] * V[0] + M[3] * V[1],
		};
	}
	
	public Point2<T> Apply ( Point2<T> value )
		=> value * this;
	public static Point2<T> operator * ( Point2<T> left, Matrix2x3<T> right ) {
		var M = right.AsSpan();
		var V = left.AsSpan();
		
		return new() {
			X = M[0] * V[0] + M[2] * V[1],
			Y = M[1] * V[0] + M[3] * V[1],
		};
	}
	
	public Vector1<T> Apply ( Vector1<T> value ) {
		var M = this.AsSpan();
		var V = value.AsSpan();
		
		return new() {
			X = M[2] + M[0] * V[0],
		};
	}
	
	public Point1<T> Apply ( Point1<T> value ) {
		var M = this.AsSpan();
		var V = value.AsSpan();
		
		return new() {
			X = M[2] + M[0] * V[0],
		};
	}
	
	public static implicit operator Span2D<T> ( Matrix2x3<T> matrix )
		=> matrix.AsSpan2D();
	public static implicit operator ReadOnlySpan2D<T> ( Matrix2x3<T> matrix )
		=> matrix.AsReadOnlySpan2D();
	
	public override string ToString () {
		return Matrix<T>.ToString( AsSpan2D() );
	}
}
