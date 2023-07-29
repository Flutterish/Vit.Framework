using System.Numerics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.Layout;

public struct RelativeSize2<T> where T : INumber<T> {
	public LayoutUnit<T> Width;
	public LayoutUnit<T> Height;

	public RelativeSize2 ( LayoutUnit<T> width, LayoutUnit<T> height ) {
		Width = width;
		Height = height;
	}

	public RelativeSize2 ( LayoutUnit<T> both ) {
		Width = Height = both;
	}

	public Size2<T> GetSize ( Size2<T> availableSpace ) => new() {
		Width = Width.GetValue( availableSpace.Width ),
		Height = Height.GetValue( availableSpace.Height )
	};

	public RelativeFlowSize2<T> ToFlow ( FlowDirection direction ) => direction.GetFlowDirection() == LayoutDirection.Horizontal
		? new( Width, Height )
		: new( Height, Width );

	public void Deconstruct ( out LayoutUnit<T> width, out LayoutUnit<T> height ) {
		width = Width;
		height = Height;
	}

	public static implicit operator RelativeSize2<T> ( Size2<T> size ) => new() {
		Width = size.Width,
		Height = size.Height
	};

	public override string ToString () {
		return $"{Width}x{Height}";
	}
}
