using System.Numerics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.Layout;

public struct SizeBounds2<T> where T : INumber<T> {
	public RelativeSize2<T> Base;
	public LayoutUnit<T>? MinWidth;
	public LayoutUnit<T>? MinHeight;
	public LayoutUnit<T>? MaxWidth;
	public LayoutUnit<T>? MaxHeight;

	public SizeBounds2 ( RelativeSize2<T> @base ) : this() {
		Base = @base;
	}
	public SizeBounds2 ( LayoutUnit<T> width, LayoutUnit<T> height ) : this() {
		Base = new( width, height );
	}

	public FlowSizeBounds2<T> ToFlow ( FlowDirection direction ) => direction.GetFlowDirection() == LayoutDirection.Horizontal
		? new() { Base = Base.ToFlow( direction ), MinFlow = MinWidth, MaxFlow = MaxWidth, MinCross = MinHeight, MaxCross = MaxHeight }
		: new() { Base = Base.ToFlow( direction ), MinCross = MinWidth, MaxCross = MaxWidth, MinFlow = MinHeight, MaxFlow = MaxHeight };

	public static implicit operator SizeBounds2<T> ( RelativeSize2<T> size ) => new() {
		Base = size
	};
	public static implicit operator SizeBounds2<T> ( Size2<T> size ) => new() {
		Base = size
	};
}
