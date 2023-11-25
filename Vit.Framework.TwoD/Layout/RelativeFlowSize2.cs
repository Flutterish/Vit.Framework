using System.Numerics;

namespace Vit.Framework.TwoD.Layout;

public struct RelativeFlowSize2<T> : IEqualityOperators<RelativeFlowSize2<T>, RelativeFlowSize2<T>, bool> where T : INumber<T> {
	public LayoutUnit<T> Flow;
	public LayoutUnit<T> Cross;

	public RelativeFlowSize2 ( LayoutUnit<T> flow, LayoutUnit<T> cross ) {
		Flow = flow;
		Cross = cross;
	}

	public RelativeFlowSize2 ( LayoutUnit<T> both ) {
		Flow = Cross = both;
	}

	public FlowSize2<T> GetSize ( FlowSize2<T> availableSpace ) => new() {
		Flow = Flow.GetValue( availableSpace.Flow ),
		Cross = Cross.GetValue( availableSpace.Cross )
	};

	public void Deconstruct ( out LayoutUnit<T> flow, out LayoutUnit<T> cross ) {
		flow = Flow;
		cross = Cross;
	}

	public static implicit operator RelativeFlowSize2<T> ( FlowSize2<T> size ) => new() {
		Flow = size.Flow,
		Cross = size.Cross
	};

	public override string ToString () {
		return $"{Flow}x{Cross}";
	}

	public static bool operator == ( RelativeFlowSize2<T> left, RelativeFlowSize2<T> right ) {
		return left.Cross == right.Cross
			&& left.Flow == right.Flow;
	}

	public static bool operator != ( RelativeFlowSize2<T> left, RelativeFlowSize2<T> right ) {
		return left.Cross != right.Cross
			|| left.Flow != right.Flow;
	}
}
