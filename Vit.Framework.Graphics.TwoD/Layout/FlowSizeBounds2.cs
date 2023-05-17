using System.Numerics;

namespace Vit.Framework.Graphics.TwoD.Layout;

public struct FlowSizeBounds2<T> where T : INumber<T> {
	public RelativeFlowSize2<T> Base;
	public RelativeFlowSize2<T>? Min;
	public RelativeFlowSize2<T>? Max;

	public static implicit operator FlowSizeBounds2<T> ( RelativeFlowSize2<T> size ) => new() {
		Base = size
	};
	public static implicit operator FlowSizeBounds2<T> ( FlowSize2<T> size ) => new() {
		Base = size
	};
}
