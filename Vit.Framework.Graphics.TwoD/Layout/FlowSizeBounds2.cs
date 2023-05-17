using System.Numerics;

namespace Vit.Framework.Graphics.TwoD.Layout;

public struct FlowSizeBounds2<T> where T : INumber<T> {
	public RelativeFlowSize2<T> Base;
	public LayoutUnit<T>? MinFlow;
	public LayoutUnit<T>? MinCross;
	public LayoutUnit<T>? MaxFlow;
	public LayoutUnit<T>? MaxCross;

	public BoundedLayoutUnit<T> Flow => new() { Base = Base.Flow, Min = MinFlow, Max = MaxFlow };
	public BoundedLayoutUnit<T> Cross => new() { Base = Base.Cross, Min = MinCross, Max = MaxCross };

	public void Deconstruct ( out BoundedLayoutUnit<T> flow, out BoundedLayoutUnit<T> cross ) {
		flow = Flow;
		cross = Cross;
	}

	public static implicit operator FlowSizeBounds2<T> ( RelativeFlowSize2<T> size ) => new() {
		Base = size
	};
	public static implicit operator FlowSizeBounds2<T> ( FlowSize2<T> size ) => new() {
		Base = size
	};
}

public struct BoundedLayoutUnit<T> where T : INumber<T> {
	public LayoutUnit<T> Base;
	public LayoutUnit<T>? Min;
	public LayoutUnit<T>? Max;

	public T GetValue ( T availableSpace ) {
		var value = Base.GetValue( availableSpace );
		if ( Min is LayoutUnit<T> min )
			value = T.Max( min.GetValue( availableSpace ), value );
		if ( Max is LayoutUnit<T> max )
			value = T.Min( max.GetValue( availableSpace ), value );

		return value;
	}

	public T GetValue ( T availableSpace, T min ) {
		var value = Base.GetValue( availableSpace );
		value = T.Max( min, value );

		if ( Min is LayoutUnit<T> _min )
			value = T.Max( _min.GetValue( availableSpace ), value );
		if ( Max is LayoutUnit<T> max )
			value = T.Min( max.GetValue( availableSpace ), value );

		return value;
	}
}