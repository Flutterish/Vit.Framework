namespace Vit.Framework.TwoD.Rendering.Masking;

public struct Corners<T> where T : unmanaged {
	public T TopLeft;
	public T TopRight;
	public T BottomLeft;
	public T BottomRight;

	public T All {
		set => TopLeft = TopRight = BottomLeft = BottomRight = value;
	}

	public static implicit operator Corners<T> ( T all )
		=> new() { All = all };
}
