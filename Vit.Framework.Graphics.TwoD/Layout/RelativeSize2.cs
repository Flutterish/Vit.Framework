using System.Numerics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.Layout;

public struct RelativeSize2<T> where T : INumber<T> {
	public LayoutUnit<T> Width;
	public LayoutUnit<T> Height;

	public RelativeSize2 ( LayoutUnit<T> width, LayoutUnit<T> height ) {
		Width = width;
		Height = height;
	}

	public Size2<T> GetSize ( Size2<T> availableSpace ) => new() {
		Width = Width.GetValue( availableSpace.Width ),
		Height = Height.GetValue( availableSpace.Height )
	};

	public static implicit operator RelativeSize2<T> ( Size2<T> size ) => new() {
		Width = size.Width,
		Height = size.Height
	};

	public override string ToString () {
		return $"{Width}x{Height}";
	}
}
