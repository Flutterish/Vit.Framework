using System.Numerics;

namespace Vit.Framework.Mathematics;

public struct Size2<T> : IEquatable<Size2<T>> where T : INumber<T> {
	public T Width;
	public T Height;

	public Size2 ( T width, T height ) {
		Width = width;
		Height = height;
	}

	public Size2<Y> Cast<Y> () where Y : INumber<Y> {
		return new Size2<Y>() {
			Width = Y.CreateChecked( Width ),
			Height = Y.CreateChecked( Height )
		};
	}

	public static bool operator == ( Size2<T> left, Size2<T> right ) {
		return left.Width == right.Width && left.Height == right.Height;
	}
	public static bool operator != ( Size2<T> left, Size2<T> right ) {
		return left.Width != right.Width || left.Height != right.Height;
	}

	public override bool Equals ( object? obj ) {
		return obj is Size2<T> size && size == this;
	}
	public bool Equals ( Size2<T> other ) {
		return other == this;
	}

	public override int GetHashCode () {
		return HashCode.Combine( Width, Height );
	}

	public override string ToString () {
		return $"{Width}x{Height}";
	}
}
