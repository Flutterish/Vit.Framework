using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics.LinearAlgebra;

public struct Matrix4<T> where T : unmanaged, INumber<T> {
	public T M00; public T M10; public T M20; public T M30;
	public T M01; public T M11; public T M21; public T M31;
	public T M02; public T M12; public T M22; public T M32;
	public T M03; public T M13; public T M23; public T M33;

	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref M00, 16 );
	public Span2D<T> AsSpan2D () => new( AsSpan(), 4, 4 );

	public Matrix4 ( ReadOnlySpan2D<T> data ) {
		data.Flat.CopyTo( this.AsSpan() );
	}

	public static readonly Matrix4<T> Identity = new() {
		M00 = T.MultiplicativeIdentity,
		M11 = T.MultiplicativeIdentity,
		M22 = T.MultiplicativeIdentity,
		M33 = T.MultiplicativeIdentity
	};
}
