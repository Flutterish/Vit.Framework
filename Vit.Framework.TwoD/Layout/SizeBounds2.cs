using System.Numerics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.Layout;

public struct SizeBounds2<T> : IInterpolatable<SizeBounds2<T>, T> where T : INumber<T> {
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

	static LayoutUnit<T>? interpolateMax ( LayoutUnit<T>? from, LayoutUnit<T>? to, T time ) {
		if ( time >= T.One )
			return to;

		if ( from == null && to == null )
			return null;

		if ( from != null && to != null )
			return from.Value.Lerp( to.Value, time );

		if ( from != null ) {
			var mult = T.One / (T.One - time);
			return new() {
				Absolute = from.Value.Absolute * mult,
				Relative = from.Value.Relative * mult
			};
		}
		else {
			var mult = T.One / time;
			return new() {
				Absolute = to!.Value.Absolute * mult,
				Relative = to!.Value.Relative * mult
			};
		}
	}

	public SizeBounds2<T> Lerp ( SizeBounds2<T> goal, T time ) {
		return new() {
			Base = Base.Lerp( goal.Base, time ),
			MinWidth = (MinWidth ?? default).Lerp( goal.MinWidth ?? default, time ),
			MinHeight = (MinHeight ?? default).Lerp( goal.MinHeight ?? default, time ),
			MaxWidth = interpolateMax( MaxWidth, goal.MaxWidth, time ),
			MaxHeight = interpolateMax( MaxHeight, goal.MaxHeight, time )
		};
	}
}
