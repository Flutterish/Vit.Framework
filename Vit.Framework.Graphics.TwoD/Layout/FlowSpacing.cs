using System.Numerics;

namespace Vit.Framework.Graphics.TwoD.Layout;

public struct FlowSpacing<T> where T : INumber<T> {
	public T CrossEnd;
	public T CrossStart;
	public T FlowStart;
	public T FlowEnd;

	public T Cross {
		get => CrossEnd + CrossStart;
		set => CrossEnd = CrossStart = value;
	}
	public T Flow {
		get => FlowStart + FlowEnd;
		set => FlowStart = FlowEnd = value;
	}

	public FlowSpacing ( T all ) {
		CrossEnd = CrossStart = FlowStart = FlowEnd = all;
	}
}
