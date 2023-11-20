using System.Numerics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.Rendering.Shaders.Types;
public struct UniformRectangle<T> where T : INumber<T> {
	public T X;
	public T Y;
	public T Width;
	public T Height;

	public UniformRectangle ( Point2<T> position, Size2<T> size ) {
		Position = position;
		Size = size;
	}

	public Point2<T> Position {
		readonly get => new( X, Y );
		set {
			X = value.X;
			Y = value.Y;
		}
	}

	public Size2<T> Size {
		readonly get => new( Width, Height );
		set {
			Width = value.Width;
			Height = value.Height;
		}
	}

	public static implicit operator UniformRectangle<T> ( AxisAlignedBox2<T> aab ) {
		return new() {
			X = aab.MinX,
			Y = aab.MinY,
			Width = aab.Width,
			Height = aab.Height
		};
	}
}
