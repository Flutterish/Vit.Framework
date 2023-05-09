/// This file [Matrix2.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.MatrixTemplate and parameter (2, 2) (System.ValueTuple`2[System.Int32,System.Int32])
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics.LinearAlgebra.Generic;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Matrix2<T> where T : INumber<T> {
	public T M00; public T M10;
	public T M01; public T M11;
	
	#nullable disable
	public Matrix2 ( ReadOnlySpan2D<T> data ) {
		data.CopyTo( this.AsSpan2D() );
	}
	#nullable restore
	
	public Matrix2 (
		T m00, T m10, 
		T m01, T m11
	) {
		M00 = m00; M10 = m10; 
		M01 = m01; M11 = m11; 
	}
	
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref M00, 4 );
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref M00, 4 );
	public ReadOnlySpan2D<T> AsReadOnlySpan2D () => new( AsReadOnlySpan(), 2, 2 );
	public Span2D<T> AsSpan2D () => new( AsSpan(), 2, 2 );
	
	public Matrix<T> AsUnsized () => new Matrix<T>( AsSpan2D() );
	
	public static readonly Matrix2<T> Identity = new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity
	};
	
	public static Matrix2<T> CreateScale ( Axes2<T> axes )
		=> CreateScale( axes.X, axes.Y );
	public static Matrix2<T> CreateScale ( T x, T y ) => new() {
		M00 = x,
		M11 = y
	};
	public static Matrix2<T> CreateScale ( Axes1<T> axes )
		=> CreateScale( axes.X );
	public static Matrix2<T> CreateScale ( T x ) => new() {
		M00 = x,
		M11 = T.MultiplicativeIdentity
	};
	
	public static Matrix2<T> CreateTranslation ( Vector1<T> vector )
		=> CreateTranslation( vector.X );
	public static Matrix2<T> CreateTranslation ( T x ) => new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M01 = x
	};
	
	public static Matrix2<T> CreateShear ( Axes2<T> shear )
		=> CreateShear( shear.X, shear.Y );
	public static Matrix2<T> CreateShear ( T x, T y ) => new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		
		M01 = x,
		M10 = y
	};
	
	public static Matrix2<T> CreateRotation<TAngle> ( TAngle angle ) where TAngle : IAngle<TAngle, T> {
		T sin = TAngle.Sin( angle ), cos = TAngle.Cos( angle );
		
		return new() {
			M00 = cos,
			M01 = -sin,
			M10 = sin,
			M11 = cos,
		};
	}
	
	public Matrix2<T> Transposed => new() {
		M00 = M00,
		M10 = M01,
		M01 = M10,
		M11 = M11,
	};
	
	public Matrix2<T> CofactorCheckerboard {
		get {
			var M = AsReadOnlySpan();
			return new() {
				M00 = M[0],
				M01 = -M[2],
				M10 = -M[1],
				M11 = M[3],
			};
		}
	}
	
	public Matrix2<T> Minors {
		get {
			var M = AsReadOnlySpan();
			return new() {
				M00 = M[3],
				M10 = M[2],
				M01 = M[1],
				M11 = M[0],
			};
		}
	}
	
	public T Determinant {
		get {
			var M = AsReadOnlySpan();
			return M[0] * M[3]
				- M[1] * M[2];
		}
	}
	
	public Matrix2<T> Inversed {
		get {
			var M = AsReadOnlySpan();
			var invDet = T.MultiplicativeIdentity / Determinant;
			return new() {
				M00 = (M[3]) * invDet,
				M10 = (-M[1]) * invDet,
				M01 = (-M[2]) * invDet,
				M11 = (M[0]) * invDet,
			};
		}
	}
	
	public static Matrix2<T> operator * ( Matrix2<T> left, Matrix2<T> right ) {
		var A = left.AsSpan();
		var B = right.AsSpan();
		
		return new() {
			M00 = A[0] * B[0] + A[1] * B[2],
			M10 = A[0] * B[1] + A[1] * B[3],
			M01 = A[2] * B[0] + A[3] * B[2],
			M11 = A[2] * B[1] + A[3] * B[3],
		};
	}
	
	public Vector2<T> Apply ( Vector2<T> value )
		=> value * this;
	public static Vector2<T> operator * ( Vector2<T> left, Matrix2<T> right ) {
		var M = right.AsSpan();
		var V = left.AsSpan();
		
		return new() {
			X = M[0] * V[0] + M[2] * V[1],
			Y = M[1] * V[0] + M[3] * V[1],
		};
	}
	
	public Point2<T> Apply ( Point2<T> value )
		=> value * this;
	public static Point2<T> operator * ( Point2<T> left, Matrix2<T> right ) {
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
	
	public static implicit operator Span2D<T> ( Matrix2<T> matrix )
		=> matrix.AsSpan2D();
	public static implicit operator ReadOnlySpan2D<T> ( Matrix2<T> matrix )
		=> matrix.AsReadOnlySpan2D();
	
	public override string ToString () {
		return Matrix<T>.ToString( AsSpan2D() );
	}
}
