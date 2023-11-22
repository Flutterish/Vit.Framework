using System.Numerics;
using Vit.Framework.Interop;

namespace Vit.Framework.TwoD.Rendering.Masking;

public struct Corners<T> : IEqualityOperators<Corners<T>, Corners<T>, bool> where T : unmanaged {
	public T TopLeft;
	public T TopRight;
	public T BottomLeft;
	public T BottomRight;

	public T All {
		set => TopLeft = TopRight = BottomLeft = BottomRight = value;
	}

	public static implicit operator Corners<T> ( T all )
		=> new() { All = all };

	public static bool operator == ( Corners<T> left, Corners<T> right ) {
		return left.ToBytes().SequenceEqual( right.ToBytes() );
	}

	public static bool operator != ( Corners<T> left, Corners<T> right ) {
		return !left.ToBytes().SequenceEqual( right.ToBytes() );
	}

	public override string ToString () {
		return $"[TopLeft = {TopLeft}; TopRight = {TopRight}; BottomLeft = {BottomLeft}; BottomRight = {BottomRight};]";
	}
}
